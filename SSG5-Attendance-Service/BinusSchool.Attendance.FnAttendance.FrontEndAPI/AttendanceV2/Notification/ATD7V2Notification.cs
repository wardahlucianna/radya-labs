using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceEntry.Notification.ViewModels;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Persistence.AttendanceDb;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2.Notification
{
    public class ATD7V2Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<ATD7V2Notification> _logger;
        private readonly IAttendanceDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _dateTime;
        private List<ATD7V2NotificationModels> _listEvent;

        public ATD7V2Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<ATD7V2Notification> logger, IAttendanceDbContext dbContext, IDictionary<string, object> notificationData, IMachineDateTime dateTime) :
           base("ATD7", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;
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
                    Web = true
                }
            };

            return Task.CompletedTask;
        }

        protected override async Task Prepare()
        {
            try
            {
                var hostUrl = Configuration.GetSection("ClientApp:Web:Host").Get<string>();
                var attendanceUrlBase = $"{hostUrl}/attendance/detaileventattendancev2";
                _notificationData = new Dictionary<string, object>();
                _listEvent = new List<ATD7V2NotificationModels>();

                var school = await _dbContext.Entity<MsSchool>().Where(x => x.Id == IdSchool).FirstOrDefaultAsync(CancellationToken);
                if (school != null)
                    _notificationData["schoolName"] = school.Name;

                var idAcademicYear = await _dbContext.Entity<MsPeriod>()
                               .Include(e => e.Grade).ThenInclude(e => e.Level).ThenInclude(e => e.AcademicYear)
                               .Where(x => _dateTime.ServerTime.Date >= x.StartDate && _dateTime.ServerTime.Date <= x.EndDate)
                               .Where(x => x.Grade.Level.AcademicYear.IdSchool == IdSchool)
                               .Select(x => x.Grade.Level.IdAcademicYear)
                               .FirstOrDefaultAsync(CancellationToken);

                var listEvent = await _dbContext.Entity<TrEvent>()
                               .Include(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForAttendanceStudents).ThenInclude(e => e.EventIntendedForAtdCheckStudents).ThenInclude(e => e.UserEventAttendance2s)
                               .Include(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForAttendanceStudents).ThenInclude(e => e.EventIntendedForAtdPICStudents)
                               .Where(x => x.IdAcademicYear == idAcademicYear
                                            && x.StatusEvent == "Approved"
                                            && x.EventDetails.Any(e => e.StartDate <= _dateTime.ServerTime.Date)
                                            && x.EventIntendedFor.Any(e => e.EventIntendedForAttendanceStudents.Any(f => f.IsSetAttendance == true))
                                            && x.EventIntendedFor.Any(e => e.EventIntendedForAttendanceStudents.Any(f => f.EventIntendedForAtdPICStudents.Any()))
                               )
                               .Select(x => new ATD7V2NotificationModels
                               {
                                   IdEvent = x.Id,
                                   EventName = x.Name,
                                   StartDate = x.EventDetails.Min(e => e.StartDate),
                                   EndDate = x.EventDetails.Max(e => e.EndDate),
                                   AttendanceCheckName = x.EventIntendedFor.SelectMany(e => e.EventIntendedForAttendanceStudents.SelectMany(f => f.EventIntendedForAtdCheckStudents.Where(d => d.IsPrimary && !d.UserEventAttendance2s.Any()).Select(d => d.CheckName))).FirstOrDefault(),
                                   AttendanceTime = x.EventIntendedFor.SelectMany(e => e.EventIntendedForAttendanceStudents.SelectMany(f => f.EventIntendedForAtdCheckStudents.Where(d => d.IsPrimary && !d.UserEventAttendance2s.Any()).Select(d => d.Time))).FirstOrDefault(),
                                   Link = $"{attendanceUrlBase}?idEvent={x.Id}",
                                   IdUser = x.EventIntendedFor.SelectMany(e => e.EventIntendedForAttendanceStudents.SelectMany(f => f.EventIntendedForAtdPICStudents.Select(d => d.IdUser))).FirstOrDefault(),
                               })
                               .ToListAsync(CancellationToken);

                _listEvent.AddRange(listEvent);
                PushNotificationData["action"] = "ATD_ENTRY";
                IdUserRecipients = _listEvent.Select(x => x.IdUser).Distinct().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        protected override async Task SendPushNotification()
        {
            var tokens = await _dbContext.Entity<MsUserPlatform>()
                .Where(x
                    => IdUserRecipients.Contains(x.IdUser) && x.FirebaseToken != null
                    && NotificationConfig.EnPush.AllowedPlatforms.Contains(x.AppPlatform))
                .Select(x => new { x.FirebaseToken, x.IdUser })
                .ToListAsync(CancellationToken);

            if (!EnsureAnyPushTokens(tokens.Select(e => e.FirebaseToken).ToList()))
                return;

            var SendPushNotificationTaks = new List<Task>();

            var tokenByUser = tokens.Where(x => IdUserRecipients.Contains(x.IdUser)).Select(x => x.FirebaseToken).ToList();

            if (!EnsureAnyPushTokens(tokenByUser))
                return;

            var PushTemplate = Handlebars.Compile(NotificationTemplate.Push);
            var PushBody = PushTemplate(_notificationData);

            var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
            var TitleBody = TitleTemplate(_notificationData);

            var message = new MulticastMessage
            {
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = TitleBody,
                    Body = TitleBody
                },
                Tokens = tokenByUser,
                Data = (IReadOnlyDictionary<string, string>)PushNotificationData
            };

            // send push notification
            SendPushNotificationTaks.Add(NotificationManager.SendPushNotification(message));

            await Task.WhenAll(SendPushNotificationTaks);
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

                var PushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                var PushBody = PushTemplate(_notificationData);

                var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var TitleBody = TitleTemplate(_notificationData);

                saveNotificationTasks.Add(NotificationManager.SaveNotification(
                    CreateNotificationHistory(
                        idUserRecipients,
                        isBlast,
                        TitleBody,
                        PushBody)));

                await Task.WhenAll(saveNotificationTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        protected override async Task SendEmailNotification()
        {
            var User = await _dbContext.Entity<MsUser>()
                  .Where(x => IdUserRecipients.Contains(x.Id))
                  .Select(x => new
                  {
                      x.Id,
                      EmailAddress = new EmailAddress(x.Email, x.DisplayName)
                  })
                  .ToListAsync(CancellationToken);

            if (!User.Any())
                return;

            var sendEmailTasks = new List<Task>();

            foreach (var idUser in IdUserRecipients)
            {
                var GetUserById = User.Where(e => e.Id == idUser).FirstOrDefault();
                if (GetUserById == null)
                    continue;

                var dataTable = string.Empty;
                var count = 1;

                foreach (var item in _listEvent.Where(x => x.IdUser == idUser).ToList())
                {
                    dataTable += "<tr>" +
                                    "<td>" + count + "</td>" +
                                    "<td>" + item.EventName + "</td>" +
                                    "<td>" + item.StartDate.ToString("dd/MM/yyyy HH:mm") + "</td>" +
                                    "<td>" + item.EndDate.ToString("dd/MM/yyyy HH:mm") + "</td>" +
                                    "<td>" + item.AttendanceCheckName + "</td>" +
                                    "<td>" + item.AttendanceTime + "</td>" +
                                    "<td><a href='" + item.Link + "'>Click Here</a></td>" +
                                "</tr>";
                    count++;
                }

                _notificationData["receiverName"] = GetUserById.EmailAddress.Name;
                _notificationData["dateValid"] = $"{_dateTime.ServerTime:dd MMMM yyyy hh:mm tt} (GMT+{DateTimeUtil.OffsetHour})";

                var emailReplace = NotificationTemplate.Email.Replace("{{data}}", dataTable);
                var EmailTemplate = Handlebars.Compile(emailReplace);
                var EmailBody = EmailTemplate(_notificationData);

                var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var TitleBody = TitleTemplate(_notificationData);


                var message = new SendGridMessage
                {
                    Subject = TitleBody,
                    Personalizations = new List<Personalization>
                    {
                        new Personalization { Tos = new List<EmailAddress> { GetUserById.EmailAddress } }
                    }
                };

                if (NotificationTemplate.EmailIsHtml)
                    message.HtmlContent = EmailBody;
                else
                    message.PlainTextContent = EmailBody;

                sendEmailTasks.Add(NotificationManager.SendEmail(message));
            }
            // send batch email
            await Task.WhenAll(sendEmailTasks);
        }
    }
}
