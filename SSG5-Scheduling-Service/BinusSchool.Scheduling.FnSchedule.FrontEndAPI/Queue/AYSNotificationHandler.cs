using System;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using BinusSchool.Scheduling.FnSchedule.StudentEnrollment.Notification;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnSchedule.Queue
{
    public class AYSNotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AYSNotificationHandler> _logger;

        public AYSNotificationHandler(IServiceProvider serviceProvider, ILogger<AYSNotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(AYSNotification))]
        public Task AYSNotification([QueueTrigger("notification-ays-schedule")] string queueItem, CancellationToken cancellationToken)
        {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));
            
            var handler = notification.IdScenario switch
            {
                "AYS8" => _serviceProvider.GetService<AYS8Notification>(),
                "AYS9" => _serviceProvider.GetService<AYS9Notification>(),
                _ => default(IFunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");
            
            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
