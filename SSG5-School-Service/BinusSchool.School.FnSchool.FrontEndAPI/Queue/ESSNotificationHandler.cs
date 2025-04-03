using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using BinusSchool.School.FnSchool.SurveySummary.Notification;

namespace BinusSchool.School.FnSchool.Queue
{
    public class ESSNotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ESSNotificationHandler> _logger;

        public ESSNotificationHandler(IServiceProvider serviceProvider, ILogger<ESSNotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(ESSNotification))]
        public Task ESSNotification([QueueTrigger("notification-ess")] string queueItem, CancellationToken cancellationToken)
        {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));

            var handler = notification.IdScenario switch
            {
                "ESS1" => _serviceProvider.GetService<ESS1Notification>(),
                "ESS2" => _serviceProvider.GetService<ESS2Notification>(),
                _ => default(IFunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");

            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
