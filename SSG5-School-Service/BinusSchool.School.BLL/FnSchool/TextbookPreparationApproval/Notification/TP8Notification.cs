using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationApproval;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.User;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.School.FnSchool.TextbookPreparationApproval.Notification
{
    public class TP8Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<TP8Notification> _logger;
        private readonly ISchoolDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public TP8Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<TP8Notification> logger, ISchoolDbContext dbContext, IDictionary<string, object> notificationData) :
           base("TP8", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;

            PushNotificationData["action"] = "TP_DECLINED";
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
                var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/textbookapproval/detail";

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

            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailTextbookApproval").Value;
            var EmailTextbookApproval = JsonConvert.DeserializeObject<GetEmailTextbookApprovalResult>(JsonConvert.SerializeObject(Object));

            var SendPushNotificationTaks = new List<Task>();
            foreach (var item in EmailTextbookApproval.Textbooks)
            {
                _notificationData["Link"] = $"{_notificationData["UrlBase"]}?Id={item.Id}";
                PushNotificationData["id"] = item.Id;

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
                    Tokens = tokens.Where(e => e.IdUser == item.IdUserApprover).Select(e => e.FirebaseToken).ToList(),
                    Data = (IReadOnlyDictionary<string, string>)PushNotificationData
                };

                GeneratedTitle = TitleBody;
                GeneratedContent = PushBody;
                await Task.WhenAll(SendPushNotificationTaks);
            }
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailTextbookApproval").Value;
            var EmailTextbookApproval = JsonConvert.DeserializeObject<GetEmailTextbookApprovalResult>(JsonConvert.SerializeObject(Object));
            var saveNotificationTasks = new List<Task>();

            foreach (var item in EmailTextbookApproval.Textbooks)
            {
                _notificationData["Link"] = $"{_notificationData["UrlBase"]}?Id={item.Id}";
                PushNotificationData["id"] = item.Id;

                var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                GeneratedContent = pushTemplate(_notificationData);

                var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
                GeneratedTitle = pushTitle(_notificationData);

                saveNotificationTasks.Add(NotificationManager.SaveNotification(
                CreateNotificationHistory(
                    new List<string> { item.IdUserCreated },
                    isBlast,
                GeneratedTitle ?? NotificationTemplate.Title,
                GeneratedContent ?? NotificationTemplate.Push)));
                await Task.WhenAll(saveNotificationTasks);
            }
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

            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailTextbookApproval").Value;
            var EmailTextbookApproval = JsonConvert.DeserializeObject<GetEmailTextbookApprovalResult>(JsonConvert.SerializeObject(Object));

            EmailTextbookApproval.Textbooks.ForEach(e => e.Link = $"{_notificationData["UrlBase"]}?Id={e.Id}");

            foreach (var ItemUser in GetUser)
            {
                _notificationData["Data"] = EmailTextbookApproval.Textbooks.Where(e => e.IdUserCreated == ItemUser.Id).ToList();
                _notificationData["Staff"] = ItemUser.EmailAddress.Name;
                _notificationData["Subject"] = EmailTextbookApproval.Textbooks.Select(e=>e.Subject).FirstOrDefault();
                _notificationData["Grade"] = EmailTextbookApproval.Textbooks.Select(e=>e.Grade).FirstOrDefault();
                _notificationData["Title"] = EmailTextbookApproval.Textbooks.Select(e=>e.Title).FirstOrDefault();
                _notificationData["Author"] = EmailTextbookApproval.Textbooks.Select(e=>e.Author).FirstOrDefault();
                _notificationData["ISBN"] = EmailTextbookApproval.Textbooks.Select(e=>e.Isbn).FirstOrDefault();
                _notificationData["Notes"] = EmailTextbookApproval.Textbooks.Select(e=>e.Note).FirstOrDefault();
                _notificationData["Status"] = EmailTextbookApproval.Textbooks.Select(e=>e.Status).FirstOrDefault();

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
                                ItemUser.EmailAddress
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
