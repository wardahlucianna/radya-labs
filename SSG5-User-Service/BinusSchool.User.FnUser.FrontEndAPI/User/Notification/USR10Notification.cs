using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BinusSchool.User.FnUser.User.Notification
{
    public class USR10Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;

        private readonly IUserDbContext _dbContext;

        public USR10Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<FunctionsNotificationHandler> logger, IUserDbContext dbContext) :
            base("USR10", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
        }

        protected override Task FetchNotificationConfig()
        {
            // TODO: get config from actual source
            NotificationConfig = new NotificationConfig
            {
                EnEmail = false,
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
            _notificationData = new Dictionary<string, object>(KeyValues);
            var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
            var pushContent = Handlebars.Compile(NotificationTemplate.Push);

            GeneratedTitle = pushTitle(_notificationData);
            GeneratedContent = pushContent(_notificationData);
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
                Tokens = tokens,
                Data = PushNotificationData as IReadOnlyDictionary<string, string>
            };

            // send push notification
            await NotificationManager.SendPushNotification(message);
        }

        protected override async Task SendEmailNotification()
        {
            var recipients = new List<Address> { new Address(KeyValues["email"] as string, KeyValues["receiverName"] as string) };

            if (!EnsureAnyEmails(recipients))
                return;

            var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
            var emailBody = emailTemplate(_notificationData);

            var message = new EmailData
            {
                FromAddress = new Address(IdSchool),
                ToAddresses = recipients,
                Subject = NotificationTemplate.Title,
                Body = emailBody,
                IsHtml = NotificationTemplate.EmailIsHtml
            };

            // send email notification
            await NotificationManager.SendSmtp(message);
        }


    }
}
