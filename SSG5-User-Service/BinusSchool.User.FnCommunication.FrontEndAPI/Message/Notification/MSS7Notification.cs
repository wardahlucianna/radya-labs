using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.Persistence.UserDb.Entities.School;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;
using FluentEmail.Core.Models;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using Newtonsoft.Json;

namespace BinusSchool.User.FnCommunication.Message.Notification
{
    public class MSS7Notification : FunctionsNotificationHandler
    {
        private readonly IUserDbContext _dbContext;

        private readonly IConfiguration _configuration;

        private readonly INotificationManager _notificationManager;

        private IDictionary<string, object> _notificationData;

        private string _idMessage;

        private string _messageType;

        private string _schoolName;

        public MSS7Notification(string idScenario, INotificationManager notificationManager, IConfiguration configuration,
            ILogger<FunctionsNotificationHandler> logger, IUserDbContext dbContext, IDictionary<string, object> notificationData) : base("MSS7", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _notificationData = notificationData;
            _configuration = configuration;
            _notificationManager = notificationManager;
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

        protected async override Task Prepare()
        {
            var url = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/communication/messageinbox";

            _schoolName = await _dbContext.Entity<MsSchool>()
                .Where(x => x.Id == IdSchool)
                .Select(x => x.Name.ToUpper())
                .FirstOrDefaultAsync(CancellationToken);

            _notificationData = new Dictionary<string, object>(KeyValues)
            {
                { "linkUrl", url },
                { "schoolName", _schoolName },
            };

            _idMessage = _notificationData.Where(x => x.Key == "id").Select(x => x.Value).FirstOrDefault().ToString();

            _messageType = _notificationData.Where(x => x.Key == "messageType").Select(x => x.Value).FirstOrDefault().ToString();

            var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
            var pushContent = Handlebars.Compile(NotificationTemplate.Push);

            GeneratedTitle = pushTitle(_notificationData);
            GeneratedContent = pushContent(_notificationData);
        }

        protected override Task SendEmailNotification()
        {
            throw new NotImplementedException();
        }

        protected async override Task SendPushNotification()
        {
            var tokens = await _dbContext.Entity<MsUserPlatform>()
                .Where(x
                    => IdUserRecipients.Contains(x.IdUser) && x.FirebaseToken != null
                    && NotificationConfig.EnPush.AllowedPlatforms.Contains(x.AppPlatform))
                .Select(x => x.FirebaseToken)
                .ToListAsync(CancellationToken);

            if (!EnsureAnyPushTokens(tokens))
                return;

            //data push notif
            PushNotificationData["action"] = "MSS_INBOX";
            PushNotificationData["id"] = _idMessage;
            PushNotificationData["mgType"] = _messageType;
            PushNotificationData["type"] = NotificationType.Message.ToString();

            // NOTE: create MulticastMessage object to send push notification
            var message = new MulticastMessage
            {
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = GeneratedTitle,
                    Body = GeneratedContent
                },
                Tokens = tokens,
                Data = (IReadOnlyDictionary<string, string>)PushNotificationData
            };

            // send push notification
            await NotificationManager.SendPushNotification(message);
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            var saveNotificationTasks = new List<Task>();

            //data push notif
            PushNotificationData["action"] = "MSS_INBOX";
            PushNotificationData["id"] = _idMessage;
            PushNotificationData["mgType"] = _messageType;
            PushNotificationData["type"] = NotificationType.Message.ToString();

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
    }
}
