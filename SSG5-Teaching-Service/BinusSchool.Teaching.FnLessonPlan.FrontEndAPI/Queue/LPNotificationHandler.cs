using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using BinusSchool.Teaching.FnLessonPlan.LessonPlan.Notification;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.Teaching.FnLessonPlan.Queue
{
    public class LPNotificationHandler
    {
        private IServiceProvider _serviceProvider;
        private ILogger<LPNotificationHandler> _logger;

        public LPNotificationHandler(IServiceProvider serviceProvider, ILogger<LPNotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        [FunctionName(nameof(LPNotification))]
        public Task LPNotification([QueueTrigger("notification-lp-lessonplan")] string queueItem, CancellationToken cancellationToken)
        {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));

            var handler = notification.IdScenario switch
            {
                //"LP1" => _serviceProvider.GetService<LP1Notification>(),
                "LP2" => _serviceProvider.GetService<LP2Notification>(),
                "LP3" => _serviceProvider.GetService<LP3Notification>(),
                "LP4" => _serviceProvider.GetService<LP4Notification>(),
                "LP5" => _serviceProvider.GetService<LP5Notification>(),
                "LP6" => _serviceProvider.GetService<LP6Notification>(),
                _ => default(IFunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");

            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
