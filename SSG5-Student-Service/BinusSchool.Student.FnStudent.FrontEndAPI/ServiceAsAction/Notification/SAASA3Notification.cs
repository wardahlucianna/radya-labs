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
using Newtonsoft.Json;

namespace BinusSchool.Student.FnStudent.ServiceAsAction.Notification
{
    public class SAASA3Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<SAASA3Notification> _logger;
        private readonly IStudentDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public SAASA3Notification(string idScenario, INotificationManager notificationManager, IConfiguration configuration, ILogger<SAASA3Notification> logger, IStudentDbContext dbContext, IDictionary<string, object> notificationData)
            : base("SAASA3", notificationManager, configuration, logger)
        {
            _notificationData = notificationData;
            _logger = logger;
            _dbContext = dbContext;
            _configuration = configuration;
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
                //var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}";

                var objectNotifUrl = KeyValues.FirstOrDefault(x => x.Key == "notifUrl").Value;
                var notifUrlData = JsonConvert.DeserializeObject<string>(JsonConvert.SerializeObject(objectNotifUrl));

                _notificationData = new Dictionary<string, object>
                {
                    { "linkUrl", notifUrlData }
                };

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return Task.CompletedTask;
            }
        }

        protected async override Task SendPushNotification()
        {
            var tokens = await _dbContext.Entity<MsUserPlatform>()
                .Where(x
                    => IdUserRecipients.Contains(x.IdUser) && x.FirebaseToken != null
                    && NotificationConfig.EnPush.AllowedPlatforms.Contains(x.AppPlatform))
                .Select(x => new { x.FirebaseToken, x.IdUser })
                .ToListAsync(CancellationToken);

            if (!EnsureAnyPushTokens(tokens.Select(y => y.FirebaseToken).ToList()))
                return;

            var objectUser = KeyValues.FirstOrDefault(x => x.Key == "user").Value;
            var userData = JsonConvert.DeserializeObject<string>(JsonConvert.SerializeObject(objectUser));

            var objectActivityName = KeyValues.FirstOrDefault(x => x.Key == "activityName").Value;
            var activityNameData = JsonConvert.DeserializeObject<string>(JsonConvert.SerializeObject(objectActivityName));

            var objectNotifUrl = KeyValues.FirstOrDefault(x => x.Key == "notifUrl").Value;
            var notifUrlData = JsonConvert.DeserializeObject<string>(JsonConvert.SerializeObject(objectNotifUrl));

            var SendPushNotificationTaks = new List<Task>();

            //data push notif

            _notificationData["user"] = userData;
            _notificationData["activityName"] = activityNameData;
            _notificationData["linkUrl"] = notifUrlData;

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

            SendPushNotificationTaks.Add(NotificationManager.SendPushNotification(message));

            await Task.WhenAll(SendPushNotificationTaks);
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {

            var objectUser = KeyValues.FirstOrDefault(x => x.Key == "user").Value;
            var userData = JsonConvert.DeserializeObject<string>(JsonConvert.SerializeObject(objectUser));

            var objectActivityName = KeyValues.FirstOrDefault(x => x.Key == "activityName").Value;
            var activityNameData = JsonConvert.DeserializeObject<string>(JsonConvert.SerializeObject(objectActivityName));

            var objectNotifUrl = KeyValues.FirstOrDefault(x => x.Key == "notifUrl").Value;
            var notifUrlData = JsonConvert.DeserializeObject<string>(JsonConvert.SerializeObject(objectNotifUrl));

            _notificationData["user"] = userData;
            _notificationData["activityName"] = activityNameData;
            _notificationData["linkUrl"] = notifUrlData;

            var PushTemplate = Handlebars.Compile(NotificationTemplate.Push);
            GeneratedContent = PushTemplate(_notificationData);

            var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
            GeneratedTitle = TitleTemplate(_notificationData);

            var saveNotificationTasks = new List<Task>();

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
