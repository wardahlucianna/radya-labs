using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceEntry.Notification.ViewModels;
using BinusSchool.Attendance.FnAttendance.Utils;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.AttendanceDb;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Employee;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Attendance.FnAttendance.AttendanceEntry.Notification
{
    public class ATD8Notification : FunctionsNotificationHandler, IDisposable
    {
        private IDictionary<string, object> _notificationData;
        private IReadOnlyCollection<UserAttendanceVm<UnsubmitSessionAttendanceVm>> _subjectTeachers;
        private IReadOnlyCollection<UserAttendanceVm<UnsubmitSessionAttendanceVm>> _subjectTeachers2;
        private readonly IAttendanceDbContext _dbContext;
        private readonly ILogger<ATD8Notification> _logger;
        private readonly IMachineDateTime _dateTime;

        public ATD8Notification(
            INotificationManager notificationManager,
            DbContextOptions<AttendanceDbContext> options,
            ILogger<ATD8Notification> logger,
            IConfiguration configuration,
            IDictionary<string, object> notificationData,
            IMachineDateTime dateTime) :
            base("ATD8", notificationManager, configuration, logger)
        {

            _dbContext = new AttendanceDbContext(options);
            _logger = logger;
            _notificationData = notificationData;
            _dateTime = dateTime;
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
                var hostUrl = Configuration.GetSection("ClientApp:Web:Host").Get<string>();
                var attendanceUrlBase = $"{hostUrl}/attendance/detailsessionattendance";
                _notificationData = new Dictionary<string, object>();

                var school = await _dbContext.Entity<MsSchool>().Where(x => x.Id == IdSchool).FirstOrDefaultAsync(CancellationToken);
                if (school != null)
                    _notificationData["schoolName"] = school.Name;

                var levelIds = await _dbContext.Entity<MsLevel>()
                                               .Include(x => x.MappingAttendances)
                                               .Where(x => x.AcademicYear.IdSchool == IdSchool
                                                          && x.MappingAttendances.Any(y => y.AbsentTerms == Common.Model.Enums.AbsentTerm.Session))
                                               .Select(x => x.Id)
                                               .ToListAsync(CancellationToken);

                var _unsubmittedSchedules = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                                          .Include(x => x.AttendanceEntries)
                                                          .Include(x => x.GeneratedScheduleStudent)
                                                          .Include(x => x.Homeroom)
                                                                .ThenInclude(x => x.AcademicYear)
                                                          .Where(x => levelIds.Contains(x.Subject.Grade.IdLevel)
                                                                     && !x.AttendanceEntries.Any()
                                                                     && x.ScheduleDate.Date <= _dateTime.ServerTime.Date)
                                                          .ToListAsync(CancellationToken);

                var _unsubmittedSchedulesExist = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                          .Include(x => x.AttendanceEntries)
                                          .Include(x => x.GeneratedScheduleStudent)
                                          .Include(x => x.Homeroom)
                                                .ThenInclude(x => x.AcademicYear)
                                          .Where(x => levelIds.Contains(x.Subject.Grade.IdLevel)
                                                     && x.AttendanceEntries.Any()
                                                     && x.ScheduleDate.Date <= _dateTime.ServerTime.Date)
                                          .ToListAsync(CancellationToken);

                if (_unsubmittedSchedulesExist.Count != 0)
                {
                    _unsubmittedSchedules = _unsubmittedSchedules.Where(x => !_unsubmittedSchedulesExist.Select(y => y.IdStudent).ToList().Contains(x.IdStudent)).ToList();
                }

                var StudentStatus = await _dbContext.Entity<TrStudentStatus>()
                                        .Where(x => _unsubmittedSchedules.Select(e=>e.GeneratedScheduleStudent.IdStudent).ToList().Contains(x.IdStudent)
                                                    && x.ActiveStatus == false
                                                )
                                        .Select(e=>e.IdStudent)
                                        .ToListAsync(CancellationToken);

                var unsubmittedSchedules = _unsubmittedSchedules
                        .Where(e=>StudentStatus.Contains(e.GeneratedScheduleStudent.IdStudent))
                        .Select(x => new { x.IdUser, x.ScheduleDate, x.SubjectName, x.ClassID, x.HomeroomName, x.IdSession, x.SessionID })
                        .Distinct()
                        .ToList();

                if (!unsubmittedSchedules.Any())
                    return;

                var subjectTeachers = new List<UserAttendanceVm<UnsubmitSessionAttendanceVm>>();
 
                var teachers = unsubmittedSchedules.GroupBy(x => x.IdUser);

                foreach (var teacher in teachers)
                {
                    var teacherDetail = await _dbContext.Entity<MsStaff>()
                                                        .Where(x => x.IdBinusian == teacher.Key)
                                                        .FirstOrDefaultAsync(CancellationToken);
                    if (teacherDetail != null)
                    {

                        subjectTeachers.Add(new UserAttendanceVm<UnsubmitSessionAttendanceVm>
                        {
                            IdUser = teacherDetail.IdBinusian,
                            EmailAddress = teacherDetail.BinusianEmailAddress,
                            FirstName = teacherDetail.FirstName,
                            LastName = teacherDetail.LastName,
                            Attendances = teacher.Select(x => new UnsubmitSessionAttendanceVm
                            {
                                Date = x.ScheduleDate,
                                Subject = x.SubjectName,
                                ClassId = x.ClassID,
                                HomeroomName = x.HomeroomName,
                                IdSession = x.IdSession,
                                SessionId = x.SessionID,
                                LinkAttendance = attendanceUrlBase + $"?idSession={x.IdSession}&date={x.ScheduleDate.ToString("MM/dd/yyyy")}&classId={x.ClassID}&currentPos=ST"
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
                PushNotificationData["action"] = "ATD_ENTRY";
                foreach (var idUser in IdUserRecipients)
                {
                    var tokenByUser = tokens.Where(x => x.IdUser == idUser).Select(x => x.FirebaseToken).ToList();

                    if (!EnsureAnyPushTokens(tokenByUser))
                        continue;

                    _notificationData["total"] = _subjectTeachers.Where(x => x.IdUser == idUser).SelectMany(x => x.Attendances).Count();
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

                var sendEmailTasks = new List<Task>();
                foreach (var idUser in IdUserRecipients)
                {
                    var dataTeacher = _subjectTeachers.First(x => x.IdUser == idUser);
                    var dataTable = string.Empty;
                    var count = 1;
                    foreach (var item in _subjectTeachers.Where(x => x.IdUser == idUser)
                                                         .SelectMany(x => x.Attendances)
                                                         .OrderBy(x => x.Date).ThenBy(x => x.Subject).ThenBy(x => x.SessionId))
                    {
                        dataTable += "<tr>" +
                                        "<td>" + count + "</td>" +
                                        "<td>" + item.Date.ToString("dd/MM/yyyy") + "</td>" +
                                        "<td>" + item.Subject + "</td>" +
                                        "<td>" + item.HomeroomName + "</td>" +
                                        "<td>" + item.SessionId + "</td>" +
                                        "<td><a href='" + item.LinkAttendance + "'>Click Here</a></td>" +
                                    "</tr>";
                        count++;
                    }

                    _notificationData["total"] = _subjectTeachers.Where(x => x.IdUser == idUser).SelectMany(x => x.Attendances).Count();
                    _notificationData["receiverName"] = $"{dataTeacher.FirstName} {dataTeacher.LastName}";
                    _notificationData["data"] = dataTable;

                    var message = new EmailData
                    {
                        Subject = NotificationTemplate.Title.ReplaceVariable(_notificationData),
                        Body = NotificationTemplate.Email.ReplaceVariable(_notificationData),
                        IsHtml = NotificationTemplate.EmailIsHtml,
                        ToAddresses = new[] { new Address(dataTeacher.EmailAddress) }.ToList()
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
                    _notificationData["total"] = _subjectTeachers.Where(x => x.IdUser == idUser).SelectMany(x => x.Attendances).Count();

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
