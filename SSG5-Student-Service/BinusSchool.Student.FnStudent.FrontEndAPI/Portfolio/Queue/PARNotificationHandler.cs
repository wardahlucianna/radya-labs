using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BinusSchool.Student.FnStudent.Portfolio.Coursework.Notification;

namespace BinusSchool.Student.FnStudent.Portfolio.Queue
{
    public class PARNotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PARNotificationHandler> _logger;

        public PARNotificationHandler(IServiceProvider serviceProvider, ILogger<PARNotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(ParNotification))]
        public Task ParNotification([QueueTrigger("notification-portofolio")] string queueItem, CancellationToken cancellationToken)
        {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));

            var handler = notification.IdScenario switch
            {
                "PAR1" => _serviceProvider.GetService<PAR1Notification>(),
                "PAR2" => _serviceProvider.GetService<PAR2Notification>(),
                "PAR3" => _serviceProvider.GetService<PAR3Notification>(),
                "PAR4" => _serviceProvider.GetService<PAR4Notification>(),
                _ => default(IFunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");

            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
