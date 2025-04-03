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
    public class ATD20Notification : FunctionsNotificationHandler, IDisposable
    {
        private IDictionary<string, object> _notificationData;
        private IReadOnlyCollection<ParentAttendanceVm<UpdateDayAttendanceVm>> _parents;
        private readonly IAttendanceDbContext _dbContext;
        private readonly ILogger<ATD20Notification> _logger;

        public ATD20Notification(
            INotificationManager notificationManager,
            DbContextOptions<AttendanceDbContext> options,
            ILogger<ATD20Notification> logger,
            IConfiguration configuration,
            IDictionary<string, object> notificationData) :
            base("ATD20", notificationManager, configuration, logger)
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
                                                 .IgnoreQueryFilters()
                                                 .ToListAsync(CancellationToken);

                if (!schedules.Any())
                    return;

                var parents = new List<ParentAttendanceVm<UpdateDayAttendanceVm>>();

                var students = schedules.GroupBy(x => new { x.GeneratedScheduleStudent.Student, x.ScheduleDate });
                foreach (var student in students)
                {
                    foreach (var parent in student.Key.Student.StudentParents)
                    {
                        parents.Add(new ParentAttendanceVm<UpdateDayAttendanceVm>
                        {
                            IdUser = parent.Parent.Id,
                            EmailAddress = parent.Parent.PersonalEmailAddress,
                            FirstName = parent.Parent.FirstName,
                            LastName = parent.Parent.LastName,
                            StudentAttendance = new UpdateDayAttendanceVm
                            {
                                OldData = new UpdateDayAttendanceDataVm
                                {
                                    BinusianId = student.Key.Student.Id,
                                    StudentName = $"{student.Key.Student.FirstName} {student.Key.Student.LastName}",
                                    Grade = student.First().Homeroom.Grade.Description,
                                    Homeroom = student.First().HomeroomName,
                                    Date = student.First().ScheduleDate,
                                    AttendanceCategory = student.First().AttendanceEntries.OrderByDescending(x => x.DateIn).Skip(1).FirstOrDefault()?.AttendanceMappingAttendance.Attendance.AttendanceCategory.ToString(),
                                    AbsenceCategory = student.First().AttendanceEntries.OrderByDescending(x => x.DateIn).Skip(1).FirstOrDefault()?.AttendanceMappingAttendance.Attendance.AbsenceCategory?.ToString(),
                                    AttendanceName = student.First().AttendanceEntries.OrderByDescending(x => x.DateIn).Skip(1).FirstOrDefault()?.AttendanceMappingAttendance.Attendance.Description,
                                    FileLink = student.First().AttendanceEntries.OrderByDescending(x => x.DateIn).Skip(1).FirstOrDefault()?.FileEvidence,
                                    Notes = student.First().AttendanceEntries.OrderByDescending(x => x.DateIn).Skip(1).FirstOrDefault()?.Notes,
                                    AbsentBy = student.First().AttendanceEntries.OrderByDescending(x => x.DateIn).Skip(1).FirstOrDefault()?.UserIn
                                },
                                NewData = new UpdateDayAttendanceDataVm
                                {
                                    BinusianId = student.Key.Student.Id,
                                    StudentName = $"{student.Key.Student.FirstName} {student.Key.Student.LastName}",
                                    Grade = student.First().Homeroom.Grade.Description,
                                    Homeroom = student.First().HomeroomName,
                                    Date = student.First().ScheduleDate,
                                    AttendanceCategory = student.First().AttendanceEntries.OrderByDescending(x => x.DateIn).FirstOrDefault()?.AttendanceMappingAttendance.Attendance.AttendanceCategory.ToString(),
                                    AbsenceCategory = student.First().AttendanceEntries.OrderByDescending(x => x.DateIn).FirstOrDefault()?.AttendanceMappingAttendance.Attendance.AbsenceCategory?.ToString(),
                                    AttendanceName = student.First().AttendanceEntries.OrderByDescending(x => x.DateIn).FirstOrDefault()?.AttendanceMappingAttendance.Attendance.Description,
                                    FileLink = student.First().AttendanceEntries.OrderByDescending(x => x.DateIn).FirstOrDefault()?.FileEvidence,
                                    Notes = student.First().AttendanceEntries.OrderByDescending(x => x.DateIn).FirstOrDefault()?.Notes,
                                    AbsentBy = student.First().AttendanceEntries.OrderByDescending(x => x.DateIn).FirstOrDefault()?.UserIn
                                }
                            }
                        });
                    }
                }

                //set absent by 
                var teacherIds = parents.Select(x => x.StudentAttendance.OldData.AbsentBy).Union(parents.Select(y => y.StudentAttendance.NewData.AbsentBy)).Where(x => !string.IsNullOrEmpty(x)).Distinct();
                var teachers = await _dbContext.Entity<MsStaff>()
                                               .Where(x => teacherIds.Contains(x.IdBinusian))
                                               .ToListAsync(CancellationToken);
                foreach (var parent in parents)
                {
                    if (!string.IsNullOrEmpty(parent.StudentAttendance.OldData.AbsentBy))
                        parent.StudentAttendance.OldData.AbsentBy = teachers.Where(x => x.IdBinusian == parent.StudentAttendance.OldData.AbsentBy).Select(x => $"{x.FirstName} {x.LastName}").FirstOrDefault();
                    if (!string.IsNullOrEmpty(parent.StudentAttendance.NewData.AbsentBy))
                        parent.StudentAttendance.NewData.AbsentBy = teachers.Where(x => x.IdBinusian == parent.StudentAttendance.OldData.AbsentBy).Select(x => $"{x.FirstName} {x.LastName}").FirstOrDefault();
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

                    _notificationData["date"] = dataParent.StudentAttendance.OldData.Date.ToString("dd/MM/yyyy");
                    _notificationData["binusianId"] = dataParent.StudentAttendance.OldData.BinusianId;
                    _notificationData["studentName"] = dataParent.StudentAttendance.OldData.StudentName;
                    _notificationData["homeroom"] = dataParent.StudentAttendance.OldData.Homeroom;
                    _notificationData["absenceName"] = dataParent.StudentAttendance.OldData.AttendanceName;
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

                if (envName == "Staging")
                {
                    //bccs.Add(new Address("bsslog.prod@gmail.com", "bsslog.prod@gmail.com"));
                    // bccs.Add(new Address("group-itdevelopmentschools@binus.edu", "group-itdevelopmentschools@binus.edu"));
                    bccs.Add(new Address("itdevschool@binus.edu", "itdevschool@binus.edu"));
                }
                var sendEmailTasks = new List<Task>();
                foreach (var idUser in IdUserRecipients)
                {
                    var dataParent = _parents.First(x => x.IdUser == idUser);
                    var dataOld = "<tr>" +
                                        "<td>" + dataParent.StudentAttendance.OldData.StudentName + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.OldData.BinusianId + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.OldData.Grade + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.OldData.Homeroom + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.OldData.Date.ToString("dd/MM/yyyy") + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.OldData.AttendanceCategory + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.OldData.AbsenceCategory + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.OldData.AttendanceName + "</td>" +
                                        "<td><a href='" + dataParent.StudentAttendance.OldData.FileLink + "'>" + (!string.IsNullOrEmpty(dataParent.StudentAttendance.OldData.FileLink) ? "Download file" : "") + "</a></td>" +
                                        "<td>" + dataParent.StudentAttendance.OldData.Notes + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.OldData.AbsentBy + "</td>" +
                                    "</tr>";
                    var dataNew = "<tr>" +
                                        "<td>" + dataParent.StudentAttendance.NewData.StudentName + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.NewData.BinusianId + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.NewData.Grade + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.NewData.Homeroom + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.NewData.Date.ToString("dd/MM/yyyy") + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.NewData.AttendanceCategory + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.NewData.AbsenceCategory + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.NewData.AttendanceName + "</td>" +
                                        "<td><a href='" + dataParent.StudentAttendance.NewData.FileLink + "'>" + (!string.IsNullOrEmpty(dataParent.StudentAttendance.NewData.FileLink) ? "Download file" : "") + "</a></td>" +
                                        "<td>" + dataParent.StudentAttendance.NewData.Notes + "</td>" +
                                        "<td>" + dataParent.StudentAttendance.NewData.AbsentBy + "</td>" +
                                    "</tr>";

                    _notificationData["date"] = dataParent.StudentAttendance.OldData.Date.ToString("dd/MM/yyyy");
                    _notificationData["binusianId"] = dataParent.StudentAttendance.OldData.BinusianId;
                    _notificationData["studentName"] = dataParent.StudentAttendance.OldData.StudentName;
                    _notificationData["homeroom"] = dataParent.StudentAttendance.OldData.Homeroom;
                    _notificationData["emailReceiver"] = dataParent.EmailAddress;
                    _notificationData["receiverName"] = $"{dataParent.FirstName} {dataParent.LastName}";
                    _notificationData["dataOld"] = dataOld;
                    _notificationData["dataNew"] = dataOld;

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
                    Type = AttendanceSendEmailType.Update
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
