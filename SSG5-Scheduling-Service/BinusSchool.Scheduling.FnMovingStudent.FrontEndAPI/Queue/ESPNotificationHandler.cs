using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using BinusSchool.Scheduling.FnMovingStudent.StudentProgramme.Notification;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnMovingStudent.Queue
{
    public class ESPNotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ESPNotificationHandler> _logger;

        public ESPNotificationHandler(IServiceProvider serviceProvider, ILogger<ESPNotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(ESPNotification))]
        public Task ESPNotification([QueueTrigger("notification-esp")] string queueItem, CancellationToken cancellationToken)
        {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));

            var handler = notification.IdScenario switch
            {
                "ESP1" => _serviceProvider.GetService<ESP1Notification>(),
                "ESP2" => _serviceProvider.GetService<ESP2Notification>(),
                _ => default(IFunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");

            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
