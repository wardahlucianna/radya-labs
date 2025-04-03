using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BinusSchool.Data.Model.User.FnCommunication.Message;

namespace BinusSchool.User.FnCommunication.Message.Notification
{
    public class EHN2Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<EHN2Notification> _logger;
        private readonly IUserDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public EHN2Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<EHN2Notification> logger, IUserDbContext dbContext, IDictionary<string, object> notificationData) :
           base("EHN2", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;

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
            try
            {
                var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/communication/messagedetailmailinglist";

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

            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetDetailGroupMailingList").Value;
            var DetailMailingList = JsonConvert.DeserializeObject<GetGroupMailingListDetailsResult>(JsonConvert.SerializeObject(Object));

            _notificationData["GroupName"] = DetailMailingList.GroupName;
            _notificationData["OwnerGroup"] = DetailMailingList.OwnerGroup;

            var SendPushNotificationTask = new List<Task>();
            var PushTemplate = Handlebars.Compile(NotificationTemplate.Push);
            var PushBody = PushTemplate(_notificationData);

            var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
            var TitleBody = TitleTemplate(_notificationData);

            PushNotificationData["id"] = DetailMailingList.Id;
            PushNotificationData["action"] = "MS_MAILING_DETAIL";

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

            await Task.WhenAll(SendPushNotificationTask);
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetDetailGroupMailingList").Value;
            var DetailMailingList = JsonConvert.DeserializeObject<GetGroupMailingListDetailsResult>(JsonConvert.SerializeObject(Object));

            _notificationData["GroupName"] = DetailMailingList.GroupName;
            _notificationData["OwnerGroup"] = DetailMailingList.OwnerGroup;

            var saveNotificationTasks = new List<Task>();

            PushNotificationData["id"] = DetailMailingList.Id;
            PushNotificationData["action"] = "MS_MAILING_DETAIL";

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

        protected override Task SendEmailNotification()
        {
            throw new NotImplementedException();
        }
    }
}
