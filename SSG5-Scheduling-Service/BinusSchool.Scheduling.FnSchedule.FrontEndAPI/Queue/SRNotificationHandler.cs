using System;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Scheduling.FnSchedule.ScheduleRealization.Notification;
using BinusSchool.Common.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnSchedule.Queue
{
    public class SRNotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SRNotificationHandler> _logger;

        public SRNotificationHandler(IServiceProvider serviceProvider, ILogger<SRNotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(SRNotificationHandler))]
        public Task TPNotification([QueueTrigger("notification-sr")] string queueItem, CancellationToken cancellationToken)
        {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));

            var handler = notification.IdScenario switch
            {
                "SR1" => _serviceProvider.GetService<SR1Notification>(),
                "SR2" => _serviceProvider.GetService<SR2Notification>(),
                "SR3" => _serviceProvider.GetService<SR3Notification>(),
                "SR4" => _serviceProvider.GetService<SR4Notification>(),
                "SR5" => _serviceProvider.GetService<SR5Notification>(),
                "SR6" => _serviceProvider.GetService<SR6Notification>(),
                "SR7" => _serviceProvider.GetService<SR7Notification>(),
                "SR8" => _serviceProvider.GetService<SR8Notification>(),
                "SR9" => _serviceProvider.GetService<SR9Notification>(),
                "SR10" => _serviceProvider.GetService<SR10Notification>(),
                _ => default(IFunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");

            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
