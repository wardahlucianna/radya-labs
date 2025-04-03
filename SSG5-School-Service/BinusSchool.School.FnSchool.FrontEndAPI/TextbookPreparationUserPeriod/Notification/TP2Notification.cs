using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationUserPeriod;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.User;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.School.FnSchool.TextbookPreparationUserPeriod.Notification
{
    public class TP2Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<TP2Notification> _logger;
        private readonly ISchoolDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public TP2Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<TP2Notification> logger, ISchoolDbContext dbContext, IDictionary<string, object> notificationData) :
           base("TP2", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;

            PushNotificationData["action"] = "TP_USER_PIC";
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
                var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/textbooksetting/detail";

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

            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailTextbookUserPeriod").Value;
            var EmailTextbookUserPeriod = JsonConvert.DeserializeObject<EmailAddTextbookUserPeriodResult>(JsonConvert.SerializeObject(Object));

            var SendPushNotificationTaks = new List<Task>();

            PushNotificationData["id"] = EmailTextbookUserPeriod.Id;
            _notificationData["Administrator"] = EmailTextbookUserPeriod.NamaAdministrator;

            var PushTemplate = Handlebars.Compile(NotificationTemplate.Push);
            var PushBody = PushTemplate(_notificationData);

            var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
            var TitleBody = TitleTemplate(_notificationData);


            // NOTE: create MulticastMessage object to send push notification
            var message = new MulticastMessage
            {
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = TitleBody,
                    Body = PushBody
                },
                Tokens = tokens.Select(e => e.FirebaseToken).ToList(),
                Data = (IReadOnlyDictionary<string, string>)PushNotificationData
            };

            GeneratedTitle = TitleBody;
            GeneratedContent = PushBody;

            await Task.WhenAll(SendPushNotificationTaks);
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailTextbookUserPeriod").Value;
            var EmailTextbookUserPeriod = JsonConvert.DeserializeObject<EmailAddTextbookUserPeriodResult>(JsonConvert.SerializeObject(Object));
            var saveNotificationTasks = new List<Task>();

            PushNotificationData["id"] = EmailTextbookUserPeriod.Id;
            _notificationData["Administrator"] = EmailTextbookUserPeriod.NamaAdministrator;

            var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
            GeneratedContent = pushTemplate(_notificationData);

            var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
            GeneratedTitle = pushTitle(_notificationData);

            saveNotificationTasks.Add(NotificationManager.SaveNotification(
            CreateNotificationHistory(
                idUserRecipients,
                isBlast,
            GeneratedTitle ?? NotificationTemplate.Title,
            GeneratedContent ?? NotificationTemplate.Push)));
            await Task.WhenAll(saveNotificationTasks);
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

            var sendEmailTasks = new List<Task>();

            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailTextbookUserPeriod").Value;
            var EmailTextbookUserPeriod = JsonConvert.DeserializeObject<EmailAddTextbookUserPeriodResult>(JsonConvert.SerializeObject(Object));

            foreach (var IdUser in EmailTextbookUserPeriod.IdUserEntry)
            {
                var User = GetUser.Where(e => e.Id == IdUser).FirstOrDefault();

                if (User == null)
                    continue;

                _notificationData["TextbookPIC"] = User.EmailAddress.Name;
                _notificationData["Link"] = $"{_notificationData["UrlBase"]}?Id={EmailTextbookUserPeriod.Id}";
                _notificationData["Administrator"] = EmailTextbookUserPeriod.NamaAdministrator;

                var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                var emailBody = emailTemplate(_notificationData);

                var titleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var titleBody = titleTemplate(_notificationData);

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
                                User.EmailAddress
                            },
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
