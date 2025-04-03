using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiary.Notification
{
    public class CD1Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;

        private readonly ISchedulingDbContext _dbContext;

        public CD1Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<CD1Notification> logger, ISchedulingDbContext dbContext, IDictionary<string, object> notificationData) :
    base("CD1", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
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
                    Web = true
                }
            };

            return Task.CompletedTask;
        }
        protected override Task Prepare()
        {
            _notificationData = KeyValues;

            var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
            var pushContent = Handlebars.Compile(NotificationTemplate.Push);

            GeneratedTitle = pushTitle(_notificationData);
            GeneratedContent = pushContent(_notificationData);

            return Task.CompletedTask;
        }

        protected override async Task SendPushNotification()
        {
            var tokens = await _dbContext.Entity<MsUserPlatform>()
                .Where(x
                    => IdUserRecipients.Contains(x.IdUser) && x.FirebaseToken != null
                    && NotificationConfig.EnPush.AllowedPlatforms.Contains(x.AppPlatform))
                .Select(x => x.FirebaseToken)
                .ToListAsync(CancellationToken);

            if (!EnsureAnyPushTokens(tokens))
                return;

            // NOTE: create MulticastMessage object to send push notification
            var message = new MulticastMessage
            {
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = GeneratedTitle,
                    Body = GeneratedContent
                },
                Tokens = tokens
            };

            // send push notification
            await NotificationManager.SendPushNotification(message);
        }

        protected override async Task SendEmailNotification()
        {
            var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
            var emailBody = emailTemplate(_notificationData);

            // NOTE: create SendGridMessage object to send email
            var message = new SendGridMessage
            {
                Subject = NotificationTemplate.Title,
                Personalizations = new List<Personalization>
                    {
                        new Personalization { Tos = new List<EmailAddress> { new EmailAddress(_notificationData["ReceiverEmail"] as string) } }
                    }
            };
            if (NotificationTemplate.EmailIsHtml)
                message.HtmlContent = emailBody;
            else
                message.PlainTextContent = GeneratedContent;

            await NotificationManager.SendEmail(message);
        }
    }
}
