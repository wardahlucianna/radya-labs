using System.Threading.Tasks;
using System;
using System.Threading;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BinusSchool.Scheduling.FnSchedule.ClassDiary.Notification;

namespace BinusSchool.Scheduling.FnSchedule.Queue
{
    public class CDNotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CDNotificationHandler> _logger;

        public CDNotificationHandler(IServiceProvider serviceProvider, ILogger<CDNotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(CDNotification))]
        public Task CDNotification([QueueTrigger("notification-cd-schedule")] string queueItem, CancellationToken cancellationToken)
        {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));

            var handler = notification.IdScenario switch
            {
                "CD1" => _serviceProvider.GetService<CD1Notification>(),
                "CD2" => _serviceProvider.GetService<CD2Notification>(),
                "CD3" => _serviceProvider.GetService<CD3Notification>(),
                "CD4" => _serviceProvider.GetService<CD4Notification>(),
                "CD5" => _serviceProvider.GetService<CD5Notification>(),
                _ => default(IFunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");

            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
