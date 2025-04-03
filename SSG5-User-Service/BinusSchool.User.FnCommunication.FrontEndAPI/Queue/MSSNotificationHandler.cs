using System.Threading.Tasks;
using System;
using System.Threading;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using BinusSchool.User.FnCommunication.Message.Notification;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.User.FnCommunication.Queue
{
    public class MSSNotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MSSNotificationHandler> _logger;

        public MSSNotificationHandler(IServiceProvider serviceProvider, ILogger<MSSNotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(MSSNotification))]
        public Task MSSNotification([QueueTrigger("notification-mss-communication")] string queueItem, CancellationToken cancellationToken)
         {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));

            var handler = notification.IdScenario switch
            {
                "MSS1" => _serviceProvider.GetService<MSS1Notification>(),
                "MSS2" => _serviceProvider.GetService<MSS2Notification>(),
                "MSS3" => _serviceProvider.GetService<MSS3Notification>(),
                "MSS4" => _serviceProvider.GetService<MSS4Notification>(),
                "MSS5" => _serviceProvider.GetService<MSS5Notification>(),
                "MSS6" => _serviceProvider.GetService<MSS6Notification>(),
                "MSS7" => _serviceProvider.GetService<MSS7Notification>(),
                "MSS8" => _serviceProvider.GetService<MSS8Notification>(),
                "MSS9" => _serviceProvider.GetService<MSS9Notification>(),
                "EHN3" => _serviceProvider.GetService<EHN3Notification>(),
                "EHN1" => _serviceProvider.GetService<EHN1Notification>(),
                "EHN2" => _serviceProvider.GetService<EHN2Notification>(),
                _ => default(IFunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");

            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
