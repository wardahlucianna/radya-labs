using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.User;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc.Notification
{
    public class EEN1Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly IStudentDbContext _dbContext;

        public EEN1Notification(INotificationManager notificationManager,
            IConfiguration configuration,
            ILogger<EEN1Notification> logger,
            IStudentDbContext dbContext,
            IDictionary<string, object> notificationData) : base("EEN1", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _notificationData = notificationData;

            //PushNotificationData["action"] = "GC_REPORT";
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

        protected override Task SendEmailNotification()
        {
            throw new NotImplementedException();
        }
    }
}
