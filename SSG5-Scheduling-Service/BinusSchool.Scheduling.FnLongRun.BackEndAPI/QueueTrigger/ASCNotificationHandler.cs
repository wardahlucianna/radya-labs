using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using BinusSchool.Scheduling.FnLongRun.BlobTrigger.AscTimetable.Notification;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnLongRun.QueueTrigger
{
    public class ASCNotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ASCNotificationHandler> _logger;

        public ASCNotificationHandler(IServiceProvider serviceProvider, ILogger<ASCNotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        
        [FunctionName(nameof(ASCNotification))]
        public Task ASCNotification([QueueTrigger("notification-asc-longrun")] string queueItem, CancellationToken cancellationToken)
        {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            notification.KeyValues.TryAdd("idScenario", notification.IdScenario);
            
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));

            var handler = notification.IdScenario switch
            {
                "ASC0" => _serviceProvider.GetService<ASC0Notification>(),
                "ASC1" => _serviceProvider.GetService<ASC1Notification>(),
                "ASC2" => _serviceProvider.GetService<ASC2Notification>(),
                _ => default(IFunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");

            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
