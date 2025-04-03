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
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.AttendanceDb;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
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

namespace BinusSchool.Attendance.FnAttendance.AttendanceEntry.Notification
{
    public class ATD10Notification : FunctionsNotificationHandler, IDisposable
    {
        private IDictionary<string, object> _notificationData;
        private IReadOnlyCollection<UserAttendanceVm<SubmittedDayAttendanceVm>> _subjectTeachers;
        private readonly IAttendanceDbContext _dbContext;
        private readonly ILogger<ATD10Notification> _logger;

        public ATD10Notification(
            INotificationManager notificationManager,
            DbContextOptions<AttendanceDbContext> options,
            ILogger<ATD10Notification> logger,
            IConfiguration configuration,
            IDictionary<string, object> notificationData) :
            base("ATD10", notificationManager, configuration, logger)
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
                var date = (DateTime)KeyValues["date"];
                var idHomeroom = (string)KeyValues["idHomeroom"];
                _notificationData = new Dictionary<string, object>();
                _notificationData["date"] = date.ToString("dddd, dd MMMM yyyy");

                var school = await _dbContext.Entity<MsSchool>().Where(x => x.Id == IdSchool).FirstOrDefaultAsync(CancellationToken);
                if (school != null)
                    _notificationData["schoolName"] = school.Name;

                var homeroom = await _dbContext.Entity<MsHomeroom>()
                                               .Include(x => x.AcademicYear)
                                               .Include(x => x.Grade)
                                               .Include(x => x.GradePathwayClassroom)
                                                    .ThenInclude(x => x.Classroom)
                                               .Where(x => x.Id == idHomeroom)
                                               .FirstOrDefaultAsync(CancellationToken);
                if (homeroom != null)
                {
                    _notificationData["academicYear"] = homeroom.AcademicYear.Description;
                    _notificationData["semester"] = homeroom.Semester;
                    _notificationData["homeroom"] = $"{homeroom.Grade.Code} {homeroom.GradePathwayClassroom.Classroom.Code}";
                }

                var homeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                                                      .Include(x => x.Staff)
                                                      .Where(x => x.IdHomeroom == idHomeroom && x.IsAttendance)
                                                      .FirstOrDefaultAsync(CancellationToken);

                var allSchedules = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                                   .Include(x => x.Week)
                                                   .Include(x => x.AttendanceEntries)
                                                       .ThenInclude(x => x.AttendanceMappingAttendance)
                                                           .ThenInclude(x => x.Attendance)
                                                   .Include(x => x.GeneratedScheduleStudent)
                                                       .ThenInclude(x => x.Student)
                                                   .Include(x => x.Homeroom)
                                                       .ThenInclude(x => x.Grade)
                                                   .Where(x => x.ScheduleDate.Date == date.Date
                                                               && x.IdHomeroom == idHomeroom
                                                               && x.IsGenerated)
                                                   .ToListAsync(CancellationToken);

                var schedules = allSchedules.Where(x => x.StartPeriod == allSchedules.Min(x => x.StartPeriod)).ToList();


                if (!schedules.Any())
                    return;

                var subjectTeachers = new List<UserAttendanceVm<SubmittedDayAttendanceVm>>();

                var teachers = allSchedules.GroupBy(x => x.IdUser);
                foreach (var teacher in teachers)
                {
                    var teacherDetail = await _dbContext.Entity<MsStaff>()
                                                        .Where(x => x.IdBinusian == teacher.Key)
                                                        .FirstOrDefaultAsync(CancellationToken);
                    if (teacherDetail != null)
                    {

                        subjectTeachers.Add(new UserAttendanceVm<SubmittedDayAttendanceVm>
                        {
                            IdUser = teacherDetail.IdBinusian,
                            EmailAddress = teacherDetail.BinusianEmailAddress,
                            FirstName = teacherDetail.FirstName,
                            LastName = teacherDetail.LastName,
                            Attendances = schedules.Where(x => x.AttendanceEntries.Any(y => y.Status == AttendanceEntryStatus.Submitted))
                                                   .GroupBy(x => x.GeneratedScheduleStudent)
                                                   .Select(x => new SubmittedDayAttendanceVm
                                                   {
                                                       BinusianId = x.Key.Student.Id,
                                                       StudentName = NameUtil.GenerateFullName(
                                                                        x.Key.Student.FirstName,
                                                                        x.Key.Student.MiddleName,
                                                                        x.Key.Student.LastName),
                                                       Weeks = x.Where(y => y.IdUser == teacher.Key).Select(y => y.Week.Description).ToList(),
                                                       ClassIds = x.Where(y => y.IdUser == teacher.Key).Select(y => y.ClassID).ToList(),
                                                       AttendanceName = x.First().AttendanceEntries.First(y => y.Status == AttendanceEntryStatus.Submitted).AttendanceMappingAttendance.Attendance.Description,
                                                       DateIn = x.First().AttendanceEntries.First(y => y.Status == AttendanceEntryStatus.Submitted).DateIn
                                                   }).ToList()
                        });
                    }
                    else
                        _logger.LogInformation($"Skip sending notification for teacher with id {teacher.Key}. Teacher data is not found");
                }

                _subjectTeachers = subjectTeachers;
                IdUserRecipients = subjectTeachers.Select(x => x.IdUser).Distinct();
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
                    var dataTeacher = _subjectTeachers.First(x => x.IdUser == idUser);
                    var dataTable = string.Empty;
                    foreach (var item in _subjectTeachers.Where(x => x.IdUser == idUser)
                                                         .SelectMany(x => x.Attendances).OrderBy(x=>x.StudentName))
                    {
                        dataTable += "<tr>" +
                                        "<td>" + item.BinusianId + "</td>" +
                                        "<td>" + item.StudentName + "</td>" +
                                        "<td>" + item.AttendanceName + "</td>" +
                                        "<td>" + (item.DateIn.HasValue ? item.DateIn.Value.ToString("dddd, dd MMMM yyyy t") : "") + "</td>" +
                                    "</tr>";
                    }

                    _notificationData["receiverName"] = $"{dataTeacher.FirstName} {dataTeacher.LastName}";
                    _notificationData["week"] = string.Join(", ", _subjectTeachers.Where(x => x.IdUser == idUser)
                                                                                  .SelectMany(x => x.Attendances.SelectMany(y => y.Weeks))
                                                                                  .Distinct());
                    _notificationData["classId"] = string.Join(", ", _subjectTeachers.Where(x => x.IdUser == idUser)
                                                                                     .SelectMany(x => x.Attendances.SelectMany(y => y.ClassIds))
                                                                                     .Distinct());
                    _notificationData["data"] = dataTable;

                    var message = new EmailData
                    {
                        Subject = NotificationTemplate.Title.ReplaceVariable(_notificationData),
                        Body = NotificationTemplate.Email.ReplaceVariable(_notificationData),
                        IsHtml = NotificationTemplate.EmailIsHtml,
                        ToAddresses = new[] { new Address(dataTeacher.EmailAddress) }.ToList(),
                        BccAddresses = bccs
                    };

                    sendEmailTasks.Add(NotificationManager.SendSmtp(message));
                }
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
