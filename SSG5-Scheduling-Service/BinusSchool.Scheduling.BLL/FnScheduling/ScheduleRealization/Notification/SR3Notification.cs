using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization.Notification
{
    public class SR3Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<SR3Notification> _logger;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private string _schoolName;

        public SR3Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<SR3Notification> logger, ISchedulingDbContext dbContext, IDictionary<string, object> notificationData) :
           base("SR3", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;

            PushNotificationData["action"] = "SR_SUBSTITUTE_TEACHER_BY_TEACHER";
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

        protected override Task Prepare()
        {
            try
            {
                var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/schedule/teachertracking";

                _notificationData = new Dictionary<string, object>
                {
                    { "UrlBase", UrlBase },
                };

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return Task.CompletedTask;
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

            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailScheduleRealization").Value;
            var EmailScheduleRealization = JsonConvert.DeserializeObject<GetEmailScheduleRealizationResult>(JsonConvert.SerializeObject(Object));

            // _notificationData["datanotif"] = EmailScheduleRealization;

            // var SendPushNotificationTaks = new List<Task>();
            // foreach (var item in EmailTextbook.Textbooks)
            // {
            //     _notificationData["Link"] = $"{_notificationData["UrlBase"]}?Id={item.Id}";
            //     PushNotificationData["id"] = item.Id;

            //     var PushTemplate = Handlebars.Compile(NotificationTemplate.Push);
            //     var PushBody = PushTemplate(_notificationData);

            //     var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
            //     var TitleBody = TitleTemplate(_notificationData);


            //     // NOTE: create MulticastMessage object to send push notification
            //     var message = new MulticastMessage
            //     {
            //         Notification = new FirebaseAdmin.Messaging.Notification
            //         {
            //             Title = TitleBody,
            //             Body = PushBody
            //         },
            //         Tokens = tokens.Where(e=>e.IdUser==item.IdUserApproval).Select(e => e.FirebaseToken).ToList(),
            //         Data = (IReadOnlyDictionary<string, string>)PushNotificationData
            //     };

            //     GeneratedTitle = TitleBody;
            //     GeneratedContent = PushBody;
            //     await Task.WhenAll(SendPushNotificationTaks);
            // }
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailScheduleRealization").Value;
            var EmailScheduleRealization = JsonConvert.DeserializeObject<GetEmailScheduleRealizationResult>(JsonConvert.SerializeObject(Object));
            var saveNotificationTasks = new List<Task>();

            _notificationData["Staff"] = EmailScheduleRealization;

            // foreach (var item in EmailScheduleRealization.IdRegularVenue)
            // {
            //     _notificationData["Link"] = $"{_notificationData["UrlBase"]}?Id={item.Id}";
            //     PushNotificationData["id"] = item.Id;

            //     var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
            //     GeneratedContent = pushTemplate(_notificationData);

            //     var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
            //     GeneratedTitle = pushTitle(_notificationData);

            //     saveNotificationTasks.Add(NotificationManager.SaveNotification(
            //     CreateNotificationHistory(
            //         new List<string> { item.IdUserApproval },
            //         isBlast,
            //     GeneratedTitle ?? NotificationTemplate.Title,
            //     GeneratedContent ?? NotificationTemplate.Push)));
            //     await Task.WhenAll(saveNotificationTasks);
            // }
        }

        protected override async Task SendEmailNotification()
        {
            var GetUser = await _dbContext.Entity<MsUser>()
                .Where(x => IdUserRecipients.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    EmailAddress = new EmailAddress(x.Email, x.DisplayName),
                    Name = x.DisplayName
                })
                .ToListAsync(CancellationToken);

            if (!GetUser.Any())
                return;

            _schoolName = await _dbContext.Entity<MsSchool>()
                .Where(x => x.Id == IdSchool)
                .Select(x => x.Name.ToUpper())
                .FirstOrDefaultAsync(CancellationToken);

            var sendEmailTasks = new List<Task>();

            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailScheduleRealization").Value;
            var EmailScheduleRealization = JsonConvert.DeserializeObject<GetEmailScheduleRealizationResult>(JsonConvert.SerializeObject(Object));

            var GetUserSubstituteTeacher = await _dbContext.Entity<MsUser>()
                .Where(x => EmailScheduleRealization.IdUserSubtituteTeacher.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    EmailAddress = new EmailAddress(x.Email, x.DisplayName),
                    Name = x.DisplayName
                })
                .ToListAsync(CancellationToken);

            var GetUserTeacher = await _dbContext.Entity<MsUser>()
                .Where(x => EmailScheduleRealization.IdUserTeacher.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    EmailAddress = new EmailAddress(x.Email, x.DisplayName),
                    Name = x.DisplayName
                })
                .ToListAsync(CancellationToken);
                
            var dataVenue = _dbContext.Entity<MsVenue>();

            foreach(var ItemUser in GetUser)
            {
                // _notificationData["Data"] = EmailTextbook.Textbooks.Where(e=>e.IdUserApproval==ItemUser.Id);

                _notificationData["ClassTeacher"] = ItemUser.Name;
                _notificationData["DateRange"] = EmailScheduleRealization.StartDate?.ToString("dd MMMM yyyy") + " - " + EmailScheduleRealization.EndDate?.ToString("dd MMMM yyyy");
                _notificationData["SchoolDay"] = EmailScheduleRealization.DaysOfWeek;
                _notificationData["SubstituteTeacher"] = ItemUser.Name;
                _notificationData["Date"] = EmailScheduleRealization.Date?.ToString("dd MMMM yyyy");
                _notificationData["StartDate"] = EmailScheduleRealization.StartDate?.ToString("dd MMMM yyyy");
                _notificationData["EndDate"] = EmailScheduleRealization.EndDate?.ToString("dd MMMM yyyy");
                _notificationData["Session"] = "Session " + EmailScheduleRealization.SessionID + " ( " + EmailScheduleRealization.SessionStartTime + " - " + EmailScheduleRealization.SessionEndTime + " ) ";
                _notificationData["ClassID"] = EmailScheduleRealization.ClassID;
                _notificationData["TeacherName"] = string.Join(",", GetUserTeacher.Select(x => x.Name).ToList());
                _notificationData["EntryStatus"] = "Done (by System," + EmailScheduleRealization.DateIn?.ToString("dd MMMM yyyy HH mm") + ")";
                _notificationData["RegularVenue"] = dataVenue.Where(x => x.Id == EmailScheduleRealization.IdRegularVenue).First().Description;
                _notificationData["SubstituteTeacher"] = string.Join(",", GetUserSubstituteTeacher.Select(x => x.Name).ToList());
                _notificationData["ChangeVenue"] = EmailScheduleRealization.IdChangeVenue == null ? dataVenue.Where(x => x.Id == EmailScheduleRealization.IdRegularVenue).First().Description : dataVenue.Where(x => x.Id == EmailScheduleRealization.IdChangeVenue).First().Description;
                _notificationData["NotesSubstituteTeacher"] = EmailScheduleRealization.NotesForSubtitutions;
                _notificationData["SchoolName"] = _schoolName;

                var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                var emailBody = emailTemplate(_notificationData);

                var titleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var titleBody = titleTemplate(_notificationData);

                List<EmailAddress> listCc = new List<EmailAddress>();

                if (IdSchool == "1")
                {
                    listCc = new List<EmailAddress>
                    {
                        new EmailAddress
                        {
                            Email = "sarthaningtyas@binus.edu",
                            Name = "sarthaningtyas@binus.edu"
                        },
                        new EmailAddress
                        {
                            Email = "via@binus.edu",
                            Name = "via@binus.edu"
                        }
                        ,
                        new EmailAddress
                        {
                            Email = "lrassari@binus.edu",
                            Name = "lrassari@binus.edu"
                        }
                    };
                }

                listCc.AddRange(GetUserTeacher.Select(x => x.EmailAddress).ToList());

                // NOTE: create SendGridMessage object to send email
                var message = new SendGridMessage
                {
                    Subject = titleBody,
                    Personalizations = new List<Personalization>
                    {
                        new Personalization
                        {
                            Tos = new List<EmailAddress>
                            {
                                ItemUser.EmailAddress
                            },
                            Ccs = listCc
                        }
                    }
                };

                if (NotificationTemplate.EmailIsHtml)
                    message.HtmlContent = emailBody;
                else
                    message.PlainTextContent = emailBody;

                sendEmailTasks.Add(NotificationManager.SendEmail(message));
            }

           

            // send batch email
            await Task.WhenAll(sendEmailTasks);
        }
    }
}
