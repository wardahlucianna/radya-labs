using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceEntry.Notification.ViewModels;
using BinusSchool.Attendance.FnAttendance.Utils;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.AttendanceDb;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Employee;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.AttendanceEntry.Notification
{
    public class ATD17Notification : FunctionsNotificationHandler, IDisposable
    {
        private IDictionary<string, object> _notificationData;
        private IReadOnlyCollection<ParentAttendanceVm<LateSessionAttendanceVm>> _parents;
        private readonly IAttendanceDbContext _dbContext;
        private readonly ILogger<ATD17Notification> _logger;

        public ATD17Notification(
            INotificationManager notificationManager,
            DbContextOptions<AttendanceDbContext> options,
            ILogger<ATD17Notification> logger,
            IConfiguration configuration,
            IDictionary<string, object> notificationData) :
            base("ATD17", notificationManager, configuration, logger)
        {

            _dbContext = new AttendanceDbContext(options);
            _logger = logger;
            _notificationData = notificationData;
        }
        protected override Task FetchNotificationConfig()
        {
            // TODO: get config from actual source
            NotificationConfig = new NotificationConfig
            {
                EnEmail = true,
                EnPush = new EnablePushConfig
                {
                    Mobile = true,
                    Web = false
                }
            };

            return Task.CompletedTask;
        }
        protected override async Task Prepare()
        {
            try
            {
                var idGeneratedScheduleLessons = JsonConvert.DeserializeObject<List<string>>(KeyValues["idGeneratedScheduleLessons"] as string);
                _notificationData = new Dictionary<string, object>();

                var school = await _dbContext.Entity<MsSchool>().Where(x => x.Id == IdSchool).FirstOrDefaultAsync(CancellationToken);
                if (school != null)
                    _notificationData["schoolName"] = school.Name;

                var schedules = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                                .Include(x => x.AttendanceEntries)
                                                     .ThenInclude(x => x.AttendanceMappingAttendance)
                                                         .ThenInclude(x => x.Attendance)
                                                .Include(x => x.GeneratedScheduleStudent)
                                                     .ThenInclude(x => x.Student)
                                                         .ThenInclude(x => x.StudentParents)
                                                            .ThenInclude(x => x.Parent)
                                                 .Include(x => x.Homeroom)
                                                     .ThenInclude(x => x.Grade)
                                                 .Where(x => idGeneratedScheduleLessons.Contains(x.Id))
                                                 .ToListAsync(CancellationToken);

                if (!schedules.Any())
                    return;

                var parents = new List<ParentAttendanceVm<LateSessionAttendanceVm>>();

                var students = schedules.GroupBy(x => x.GeneratedScheduleStudent.Student);
                foreach (var student in students)
                {
                    foreach (var parent in student.Key.StudentParents)
                    {
                        parents.Add(new ParentAttendanceVm<LateSessionAttendanceVm>
                        {
                            IdUser = parent.Parent.Id,
                            EmailAddress = parent.Parent.PersonalEmailAddress,
                            FirstName = parent.Parent.FirstName,
                            LastName = parent.Parent.LastName,
                            StudentAttendance = new LateSessionAttendanceVm
                            {
                                BinusianId = student.Key.Id,
                                StudentName = $"{student.Key.FirstName} {student.Key.LastName}",
                                Grade = student.First().Homeroom.Grade.Description,
                                Homeroom = student.First().HomeroomName,
                                Date = student.First().ScheduleDate,
                                ClassId = student.First().ClassID,
                                SessionId = student.First().SessionID,
                                AttendanceName = student.First().AttendanceEntries.FirstOrDefault()?.AttendanceMappingAttendance.Attendance.Description,
                                LateTime = student.First().AttendanceEntries.FirstOrDefault()?.LateTime,
                                FileLink = student.First().AttendanceEntries.FirstOrDefault()?.FileEvidence,
                                Notes = student.First().AttendanceEntries.FirstOrDefault()?.Notes,
                                AbsentBy = student.First().AttendanceEntries.FirstOrDefault()?.UserIn
                            }
                        });
                    }
                }

                //set absent by 
                var teacherIds = parents.Where(x => !string.IsNullOrEmpty(x.StudentAttendance.AbsentBy)).Select(x => x.StudentAttendance.AbsentBy).Distinct();
                var teachers = await _dbContext.Entity<MsStaff>()
                                               .Where(x => teacherIds.Contains(x.IdBinusian))
                                               .ToListAsync(CancellationToken);
                foreach (var parent in parents)
                {
                    if (!string.IsNullOrEmpty(parent.StudentAttendance.AbsentBy))
                        parent.StudentAttendance.AbsentBy = teachers.Where(x => x.IdBinusian == parent.StudentAttendance.AbsentBy).Select(x => $"{x.FirstName} {x.LastName}").FirstOrDefault();
                }

                _parents = parents;
                IdUserRecipients = parents.Select(x => x.IdUser).Distinct();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
        protected override async Task SendPushNotification()
        {
            try
            {
                if (IdUserRecipients is null)
                {
                    _logger.LogInformation($"Skip sending notification. No Id User Recipients");
                    return;
                }

                var tokens = await _dbContext.Entity<MsUserPlatform>()
                    .Where(x
                        => IdUserRecipients.Contains(x.IdUser) && x.FirebaseToken != null
                        && NotificationConfig.EnPush.AllowedPlatforms.Contains(x.AppPlatform))
                    .Select(x => new
                    {
                        x.IdUser,
                        x.FirebaseToken
                    }
                    )
                    .ToListAsync(CancellationToken);

                var sendPushTasks = new List<Task>();
                foreach (var idUser in IdUserRecipients)
                {
                    var tokenByUser = tokens.Where(x => x.IdUser == idUser).Select(x => x.FirebaseToken).ToList();

                    if (!EnsureAnyPushTokens(tokenByUser))
                        continue;

                    var dataParent = _parents.First(x => x.IdUser == idUser);

                    _notificationData["date"] = dataParent.StudentAttendance.Date.ToString("dd/MM/yyyy");
                    _notificationData["binusianId"] = dataParent.StudentAttendance.BinusianId;
                    _notificationData["studentName"] = dataParent.StudentAttendance.StudentName;
                    _notificationData["homeroom"] = dataParent.StudentAttendance.Homeroom;
                    _notificationData["emailReceiver"] = dataParent.EmailAddress;

                    var message = new MulticastMessage
                    {
                        Notification = new FirebaseAdmin.Messaging.Notification
                        {
                            Title = NotificationTemplate.Title.ReplaceVariable(_notificationData),
                            Body = NotificationTemplate.Push.ReplaceVariable(_notificationData)
                        },
                        Tokens = tokenByUser,
                        Data = (IReadOnlyDictionary<string, string>)PushNotificationData
                    };

                    // send push notification
                    sendPushTasks.Add(NotificationManager.SendPushNotification(message));
                }
                await Task.WhenAll(sendPushTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
        protected override async Task SendEmailNotification()
        {
            try
            {
                if (IdUserRecipients is null)
                {
                    _logger.LogInformation($"Skip sending notification. No Id User Recipients");
                    return;
                }
                var bccs = new List<Address>();

                var envName = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ??
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                if (envName == "UAT")
                {
                    //bccs.Add(new Address("bsslog.prod@gmail.com", "bsslog.prod@gmail.com"));
                    // bccs.Add(new Address("group-itdevelopmentschools@binus.edu", "group-itdevelopmentschools@binus.edu"));
                    bccs.Add(new Address("itdevschool@binus.edu", "itdevschool@binus.edu"));
                }
                var sendEmailTasks = new List<Task>();
                foreach (var idUser in IdUserRecipients)
                {
                    var dataParent = _parents.First(x => x.IdUser == idUser);
                    var dataTable = "<tr>" +
                                        "<td>" + dataParent.StudentAttendance.StudentName + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.BinusianId + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.Grade + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.Homeroom + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.Date.ToString("dd/MM/yyyy") + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.ClassId + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.SessionId + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.AttendanceName + "</td>" +
                                        "<td>" + (dataParent.StudentAttendance.LateTime.HasValue ? dataParent.StudentAttendance.LateTime.Value.ToString(@"hh\:mm") : "") + "</td>" +
                                        "<td><a href='" + dataParent.StudentAttendance.FileLink + "'>" + (!string.IsNullOrEmpty(dataParent.StudentAttendance.FileLink) ? "Download file" : "") + "</a></td>" +
                                        "<td>" + dataParent.StudentAttendance.Notes + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.AbsentBy + "</td>" +
                                    "</tr>";

                    _notificationData["date"] = dataParent.StudentAttendance.Date.ToString("dd/MM/yyyy");
                    _notificationData["binusianId"] = dataParent.StudentAttendance.BinusianId;
                    _notificationData["studentName"] = dataParent.StudentAttendance.StudentName;
                    _notificationData["homeroom"] = dataParent.StudentAttendance.Homeroom;
                    _notificationData["emailReceiver"] = dataParent.EmailAddress;
                    _notificationData["receiverName"] = $"{dataParent.FirstName} {dataParent.LastName}";
                    _notificationData["data"] = dataTable;

                    var message = new EmailData
                    {
                        Subject = NotificationTemplate.Title.ReplaceVariable(_notificationData),
                        Body = NotificationTemplate.Email.ReplaceVariable(_notificationData),
                        IsHtml = NotificationTemplate.EmailIsHtml,
                        ToAddresses = new[] { new Address(dataParent.EmailAddress) }.ToList(),
                        BccAddresses = bccs
                    };

                    sendEmailTasks.Add(NotificationManager.SendSmtp(message));
                }

                //set send email flag to schedule lesson
                var idGeneratedScheduleLessons = JsonConvert.DeserializeObject<List<string>>(KeyValues["idGeneratedScheduleLessons"] as string);
                _dbContext.Entity<TrSendEmailAttendanceEntry>().AddRange(idGeneratedScheduleLessons.Select(x => new TrSendEmailAttendanceEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    IdGeneratedScheduleLesson = x,
                    Type = AttendanceSendEmailType.Late
                }));

                await _dbContext.SaveChangesAsync(CancellationToken);

                await Task.WhenAll(sendEmailTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            try
            {
                if (idUserRecipients is null)
                {
                    _logger.LogInformation($"Skip sending notification. No Id User Recipients");
                    return;
                }

                var saveNotificationTasks = new List<Task>();
                foreach (var idUser in idUserRecipients)
                {
                    saveNotificationTasks.Add(NotificationManager.SaveNotification(
                        CreateNotificationHistory(
                            new[] { idUser },
                            isBlast,
                            NotificationTemplate.Title.ReplaceVariable(_notificationData),
                            NotificationTemplate.Push.ReplaceVariable(_notificationData))));
                }
                await Task.WhenAll(saveNotificationTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
        public void Dispose()
        {
            (_dbContext as AttendanceDbContext)?.Dispose();
        }
    }
}
