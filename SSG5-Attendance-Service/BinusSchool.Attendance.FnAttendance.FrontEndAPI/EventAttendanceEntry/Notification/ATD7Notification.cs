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
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Employee;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Attendance.FnAttendance.EventAttendanceEntry.Notification
{
    public class ATD7Notification : FunctionsNotificationHandler, IDisposable
    {
        private IDictionary<string, object> _notificationData;
        private IReadOnlyCollection<UserAttendanceVm<EventAttendanceVm>> _userPIC;
        private readonly IAttendanceDbContext _dbContext;
        private readonly ILogger<ATD7Notification> _logger;
        private readonly IMachineDateTime _dateTime;

        public ATD7Notification(
            INotificationManager notificationManager,
            DbContextOptions<AttendanceDbContext> options,
            ILogger<ATD7Notification> logger,
            IConfiguration configuration,
            IDictionary<string, object> notificationData,
            IMachineDateTime dateTime) :
            base("ATD7", notificationManager, configuration, logger)
        {
            _dbContext = new AttendanceDbContext(options);
            _logger = logger;
            _notificationData = notificationData;
            _dateTime = dateTime;
        }

        private class EventAttendanceVm
        {
            public string IdEvent { get; set; }
            public string EventName { get; set; }
            public List<EventDateVm> EventDates { get; set; }
            public List<AttendanceCheckVm> AttendanceChecks { get; set; }
            public string LinkAttendance { get; set; }

        }
        public class EventDateVm
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
        }
        private class AttendanceCheckVm
        {
            public string CheckName { get; set; }
            public DateTime Date { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
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
                var attendanceUrlBase = $"{hostUrl}/attendance/detaileventattendance";
                _notificationData = new Dictionary<string, object>();

                var school = await _dbContext.Entity<MsSchool>().Where(x => x.Id == IdSchool).FirstOrDefaultAsync(CancellationToken);
                if (school != null)
                    _notificationData["schoolName"] = school.Name;

                // var unsubmittedEvents = await _dbContext.Entity<MsEvent>()
                //                                         .Include(x => x.EventDetails)
                //                                             .ThenInclude(x => x.UserEvents)
                //                                         .Include(x => x.EventIntendedFor)
                //                                             .ThenInclude(x => x.EventIntendedForAttendanceStudents)
                //                                                 .ThenInclude(x => x.EventIntendedForAtdPICStudents)
                //                                         .Include(x => x.EventIntendedFor)
                //                                             .ThenInclude(x => x.EventIntendedForAttendanceStudents)
                //                                                 .ThenInclude(x => x.EventIntendedForAtdCheckStudents)
                //                                                     .ThenInclude(x => x.UserEventAttendances)
                //                                         .Where(x => x.EventDetails.Any(y => y.StartDate.Date <= _dateTime.ServerTime.Date)
                //                                                     && x.EventIntendedFor.EventIntendedForAttendanceStudents.Any(y => y.EventIntendedForAtdCheckStudents.Any(z => !z.UserEventAttendances.Any())))
                //                                         .ToListAsync(CancellationToken);

                // if (!unsubmittedEvents.Any())
                //     return;

                // var picEvents = new List<UserAttendanceVm<EventAttendanceVm>>();

                // var picUsers = unsubmittedEvents.SelectMany(x => x.EventIntendedFor.EventIntendedForAttendanceStudents
                //                                .SelectMany(y => y.EventIntendedForAtdPICStudents.Select(z => z.IdUser)))
                //                                .Distinct();

                // foreach (var userId in picUsers)
                // {
                //     var userDetail = await _dbContext.Entity<MsStaff>()
                //                                           .Where(x => x.IdBinusian == userId)
                //                                           .FirstOrDefaultAsync(CancellationToken);
                //     if (userDetail != null)
                //     {
                //         var userEvents = unsubmittedEvents.Where(x => x.EventIntendedFor.EventIntendedForAttendanceStudents.Any(y => y.EventIntendedForAtdPICStudents.Any(z => z.IdUser == userId)))
                //                                           .ToList();
                //         picEvents.Add(new UserAttendanceVm<EventAttendanceVm>
                //         {
                //             IdUser = userDetail.IdBinusian,
                //             EmailAddress = userDetail.BinusianEmailAddress,
                //             FirstName = userDetail.FirstName,
                //             LastName = userDetail.LastName,
                //             Attendances = userEvents.Select(x => new EventAttendanceVm
                //             {
                //                 IdEvent = x.Id,
                //                 EventName = x.Name,
                //                 EventDates = x.EventDetails.Select(y => new EventDateVm
                //                 {
                //                     StartDate = y.StartDate,
                //                     StartTime = y.StartDate.TimeOfDay,
                //                     EndDate = y.EndDate,
                //                     EndTime = y.EndDate.TimeOfDay
                //                 }).ToList(),
                //                 AttendanceChecks = x.EventIntendedFor.EventIntendedForAttendanceStudents
                //                                     .SelectMany(y => y.EventIntendedForAtdCheckStudents
                //                                                       .Where(z => !z.UserEventAttendances.Any()
                //                                                                   || z.UserEventAttendances.Count() != x.EventDetails.SelectMany(a => a.UserEvents)
                //                                                                                                                      .GroupBy(a => a.IdUser)
                //                                                                                                                      .Count())
                //                                                       .Select(z => new AttendanceCheckVm
                //                                                       {
                //                                                           CheckName = z.CheckName,
                //                                                           Date = z.StartDate,
                //                                                           StartTime = z.StartTime,
                //                                                           EndTime = z.EndTime
                //                                                       })).ToList(),
                //                 LinkAttendance = attendanceUrlBase + $"?idEvent={x.Id}",
                //             }).ToList()
                //         });

                //     }
                //     else
                //         _logger.LogInformation($"Skip sending notification for user pic {userId}. Data is not found");
                // }

                // _userPIC = picEvents;
                // IdUserRecipients = picEvents.Select(x => x.IdUser).Distinct();
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

                    _notificationData["total"] = _userPIC.Where(x => x.IdUser == idUser).SelectMany(x => x.Attendances).Count();
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
                    var dataTeacher = _userPIC.First(x => x.IdUser == idUser);
                    var dataTable = string.Empty;
                    var count = 1;
                    foreach (var item in _userPIC.Where(x => x.IdUser == idUser)
                                                 .SelectMany(x => x.Attendances)
                                                 .OrderBy(x => x.EventDates.Min(y => y.StartDate)))
                    {
                        var rowspan = item.AttendanceChecks.Count();
                        var eventDate = string.Join("</br>", item.EventDates.Select(x => string.Format("{0} - {1}", x.StartDate.ToString("dd/MM/yyyy"), x.EndDate.ToString("dd/MM/yyyy"))));

                        for (int i = 0; i < item.AttendanceChecks.Count(); i++)
                        {
                            if (i == 0)
                            {
                                dataTable += "<tr>" +
                                        "<td rowspan='" + rowspan + "' text-align='center'>" + count + "</td>" +
                                        "<td rowspan='" + rowspan + "' text-align='center'>" + item.EventName + "</td>" +
                                        "<td rowspan='" + rowspan + "' text-align='center'>" + eventDate + "</td>" +
                                        "<td>" + item.AttendanceChecks[i].CheckName + "</td>" +
                                        "<td>" + item.AttendanceChecks[i].StartTime.ToString(@"hh:mm") + " - " + item.AttendanceChecks[i].EndTime.ToString(@"hh:mm") + " (" + item.AttendanceChecks[i].Date.ToString("dd MMM") + ")</td>" +
                                        "<td rowspan='" + rowspan + "' text-align='center'><a href='" + item.LinkAttendance + "'>Click Here</a></td>" +
                                    "</tr>";
                            }
                            else
                            {
                                dataTable += "<tr>" +
                                        "<td>" + item.AttendanceChecks[i].CheckName + "</td>" +
                                        "<td>" + item.AttendanceChecks[i].StartTime.ToString(@"hh:mm") + " - " + item.AttendanceChecks[i].EndTime.ToString(@"hh:mm") + " (" + item.AttendanceChecks[i].Date.ToString("dd MMM") + ")</td>" +
                                    "</tr>";
                            }
                        }
                        count++;
                    }

                    _notificationData["total"] = _userPIC.Where(x => x.IdUser == idUser).SelectMany(x => x.Attendances).Count();
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
                    _notificationData["total"] = _userPIC.Where(x => x.IdUser == idUser).SelectMany(x => x.Attendances).Count();

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
