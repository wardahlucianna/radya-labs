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
    public class ATD9Notification : FunctionsNotificationHandler, IDisposable
    {
        private IDictionary<string, object> _notificationData;
        private IReadOnlyCollection<UserAttendanceVm<UnsubmitDayAttendanceVm>> _homeroomTeachers;
        private readonly IAttendanceDbContext _dbContext;
        private readonly ILogger<ATD9Notification> _logger;
        private readonly IMachineDateTime _dateTime;

        public ATD9Notification(
            INotificationManager notificationManager,
            DbContextOptions<AttendanceDbContext> options,
            ILogger<ATD9Notification> logger,
            IConfiguration configuration,
            IDictionary<string, object> notificationData,
            IMachineDateTime dateTime) :
            base("ATD9", notificationManager, configuration, logger)
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
                var attendanceUrlBase = $"{hostUrl}/attendance/detailhomeroomattendance";
                _notificationData = new Dictionary<string, object>();

                var school = await _dbContext.Entity<MsSchool>().Where(x => x.Id == IdSchool).FirstOrDefaultAsync(CancellationToken);
                if (school != null)
                    _notificationData["schoolName"] = school.Name;

                var levelIds = await _dbContext.Entity<MsLevel>()
                                               .Include(x => x.MappingAttendances)
                                               .Where(x => x.AcademicYear.IdSchool == IdSchool
                                                          && x.MappingAttendances.Any(y => y.AbsentTerms == Common.Model.Enums.AbsentTerm.Day))
                                               .Select(x => x.Id)
                                               .ToListAsync(CancellationToken);

                var unsubmittedSchedules = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                                          .Include(x => x.AttendanceEntries)
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
                    unsubmittedSchedules = unsubmittedSchedules.Where(x => !_unsubmittedSchedulesExist.Select(y => y.IdStudent).ToList().Contains(x.IdStudent)).ToList();
                }

                var StudentStatus = await _dbContext.Entity<TrStudentStatus>()
                        .Where(x => unsubmittedSchedules.Select(e => e.GeneratedScheduleStudent.IdStudent).ToList().Contains(x.IdStudent)
                                    && x.ActiveStatus == false
                                )
                        .Select(e => e.IdStudent)
                        .ToListAsync(CancellationToken);


                if (!unsubmittedSchedules.Any())
                    return;

                var homeroomTeachers = new List<UserAttendanceVm<UnsubmitDayAttendanceVm>>();

                var homerooms = unsubmittedSchedules.GroupBy(x => x.IdHomeroom);
                foreach (var homeroom in homerooms)
                {
                    var homeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                                                          .Include(x => x.Staff)
                                                          .Where(x => x.IdHomeroom == homeroom.Key && x.IsAttendance)
                                                          .FirstOrDefaultAsync(CancellationToken);
                    if (homeroomTeacher != null)
                    {

                        homeroomTeachers.Add(new UserAttendanceVm<UnsubmitDayAttendanceVm>
                        {
                            IdUser = homeroomTeacher.Staff.IdBinusian,
                            EmailAddress = homeroomTeacher.Staff.BinusianEmailAddress,
                            FirstName = homeroomTeacher.Staff.FirstName,
                            LastName = homeroomTeacher.Staff.LastName,
                            Attendances = homeroom.GroupBy(x => new { x.ScheduleDate, x.IdHomeroom, x.HomeroomName }).Select(x => new UnsubmitDayAttendanceVm
                            {
                                Date = x.Key.ScheduleDate,
                                HomeroomName = x.Key.HomeroomName,
                                LinkAttendance = attendanceUrlBase + $"?idHomeroom={x.Key.IdHomeroom}&date={x.Key.ScheduleDate.ToString("MM/dd/yyyy")}&currentPos=CA"
                            }).OrderBy(x => x.Date).ThenBy(x => x.HomeroomName).ToList()
                        });
                    }
                    else
                        _logger.LogInformation($"Skip sending notification for homeroom {homeroom.Key}. No Homeroom Teacher");
                }

                _homeroomTeachers = homeroomTeachers;
                IdUserRecipients = homeroomTeachers.Select(x => x.IdUser).Distinct();
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

                    _notificationData["total"] = _homeroomTeachers.Where(x => x.IdUser == idUser).SelectMany(x => x.Attendances).Count();
                    var message = new MulticastMessage
                    {
                        Notification = new FirebaseAdmin.Messaging.Notification
                        {
                            Title = NotificationTemplate.Title.ReplaceVariable(_notificationData),
                            Body = NotificationTemplate.Push.ReplaceVariable(_notificationData)
                        },
                        Tokens = tokenByUser
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
                    var dataTeacher = _homeroomTeachers.First(x => x.IdUser == idUser);
                    var dataTable = string.Empty;
                    var count = 1;
                    foreach (var item in _homeroomTeachers.Where(x => x.IdUser == idUser)
                                                          .SelectMany(x => x.Attendances)
                                                          .OrderBy(x => x.Date).ThenBy(x => x.HomeroomName))
                    {
                        dataTable += "<tr>" +
                                        "<td>" + count + "</td>" +
                                        "<td>" + item.Date.ToString("dd/MM/yyyy") + "</td>" +
                                        "<td>" + item.HomeroomName + "</td>" +
                                        "<td><a href='" + item.LinkAttendance + "'>Click Here</a></td>" +
                                    "</tr>";
                        count++;
                    }

                    _notificationData["total"] = _homeroomTeachers.Where(x => x.IdUser == idUser).SelectMany(x => x.Attendances).Count();
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
                    _notificationData["total"] = _homeroomTeachers.Where(x => x.IdUser == idUser).SelectMany(x => x.Attendances).Count();

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
