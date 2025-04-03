using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using FluentEmail.Core.Models;
using BinusSchool.Common.Model.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Common.Functions.Handler
{
    public abstract class FunctionsNotificationHandler : IFunctionsNotificationHandler
    {
        protected CancellationToken CancellationToken;
        protected NotificationConfig NotificationConfig;
        protected NotificationTemplate NotificationTemplate;
        protected IEnumerable<string> IdUserRecipients;
        protected IDictionary<string, object> KeyValues;
        protected string IdSchool, GeneratedTitle, GeneratedContent;

        protected readonly string IdScenario;
        protected readonly INotificationManager NotificationManager;
        protected readonly IConfiguration Configuration;
        protected readonly ILogger<FunctionsNotificationHandler> Logger;
        protected readonly IDictionary<string, string> PushNotificationData;

        private readonly NotificationType _notificationType;
        private readonly bool _skipGetTemplate;

        protected FunctionsNotificationHandler(string idScenario, INotificationManager notificationManager, IConfiguration configuration,
            ILogger<FunctionsNotificationHandler> logger, string action = "NONE", NotificationType notificationType = NotificationType.General, bool skipGetTemplate = false)
        {
            IdScenario = idScenario;
            NotificationManager = notificationManager;
            Configuration = configuration;
            Logger = logger;
            PushNotificationData = new Dictionary<string, string>
            {
                { "type", notificationType.ToString() },
                { "action", action },
                { "Scenario", idScenario }
            };

            _notificationType = notificationType;
            _skipGetTemplate = skipGetTemplate;
        }

        public async Task Execute(string idSchool, IEnumerable<string> idUserRecipients, IDictionary<string, object> keyValues, CancellationToken cancellationToken = default)
        {
            try
            {
                IdSchool = idSchool;
                CancellationToken = cancellationToken;

                await FetchNotificationConfig();

                // check if NotificationConfig is fetched
                if (NotificationConfig is null)
                    throw new NullReferenceException("NotificationConfig must not be null.");

                // return when no notification will send
                if (!NotificationConfig.NotificationEnabled)
                    return;

                // check if should skip get notification template
                if (!_skipGetTemplate)
                    NotificationTemplate = await NotificationManager.GetTemplate(IdSchool, IdScenario);

                IdUserRecipients = idUserRecipients;
                KeyValues = keyValues;

                await Prepare();

                // send push notification if enabled
                if (NotificationConfig.EnPush.PushEnabled)
                    await SendPushNotification();
                else
                    Logger.LogInformation("Skip sending push notification. No push token from recipients.");

                // send email notification if enabled
                if (NotificationConfig.EnEmail)
                    await SendEmailNotification();
                else
                    Logger.LogInformation("Skip sending email notification. No Email from recipients.");

                // save notification history
                await SaveNotification(IdUserRecipients, false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[{0}:{1}] {2}", IdScenario, idSchool, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// #1 Fetch notification configuration
        /// </summary>
        protected abstract Task FetchNotificationConfig();

        /// <summary>
        /// #2 Prepare data before execute send email & push notification
        /// </summary>
        protected abstract Task Prepare();

        /// <summary>
        /// #3 Send Push notification
        /// </summary>
        protected abstract Task SendPushNotification();

        /// <summary>
        /// #4 Send Email notification
        /// </summary>
        /// <returns></returns>
        protected abstract Task SendEmailNotification();

        /// <summary>
        /// #5 Save notification to notification history
        /// </summary>
        /// <param name="idUserRecipients">User that received notifications</param>
        /// <param name="isBlast">Send notification to all users</param>
        protected virtual async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            var notification = CreateNotificationHistory(idUserRecipients, isBlast,
                GeneratedTitle ?? NotificationTemplate.Title,
                GeneratedContent ?? NotificationTemplate.Push);

            await NotificationManager.SaveNotification(notification);
        }

        /// <summary>
        /// Create default notification history object based on existing data
        /// </summary>
        /// <param name="idUserRecipients">User that received notifications</param>
        /// <param name="isBlast">Send notification to all users</param>
        /// <param name="title">Notification title</param>
        /// <param name="content">Notification content</param>
        /// <returns>Notification history object</returns>
        protected NotificationHistory CreateNotificationHistory(IEnumerable<string> idUserRecipients, bool isBlast, string title = "No Title", string content = null, string id = null)
        {
            if (id != null)
                PushNotificationData["Id"] = id;

            return new NotificationHistory
            {
                IdSchool = IdSchool,
                IdFeature = NotificationTemplate.IdFeatureSchool,
                Title = title,
                Content = content,
                IsBlast = isBlast,
                Scenario = IdScenario,
                IdUserRecipients = idUserRecipients,
                NotificationType = _notificationType,
                Action = PushNotificationData["action"],
                Data = JsonConvert.SerializeObject(PushNotificationData)
            };
        }

        /// <summary>
        /// Validate push token is not empty
        /// </summary>
        /// <param name="pushTokens">Firebase token of recipients</param>
        protected bool EnsureAnyPushTokens(IReadOnlyList<string> pushTokens)
        {
            if (pushTokens is { } && pushTokens.Count != 0)
                return true;

            Logger.LogInformation("Skip sending push notification. No Firebase token from recipients.");
            return false;
        }

        /// <summary>
        /// Validate email is not empty
        /// </summary>
        /// <param name="emails">Email of recipients</param>
        protected bool EnsureAnyEmails(IEnumerable<EmailAddress> emails)
        {
            if (emails is { } && !emails.All(x => string.IsNullOrEmpty(x.Email)))
                return true;

            Logger.LogInformation("Skip sending email notification. No email from recipients.");
            return false;
        }

        /// <summary>
        /// Validate email is not empty
        /// </summary>
        /// <param name="emails">Email of recipients</param>
        protected bool EnsureAnyEmails(IEnumerable<Address> emails)
        {
            if (emails is { } && !emails.All(x => string.IsNullOrEmpty(x.EmailAddress)))
                return true;

            Logger.LogInformation("Skip sending email notification. No email from recipients.");
            return false;
        }
    }
}
