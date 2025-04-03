using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using BinusSchool.Scheduling.FnMovingStudent.MoveStudentEnrollment.Notification;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;
using BinusSchool.Scheduling.FnMovingStudent.MoveStudentHomeroom.Notification;
using Microsoft.Extensions.DependencyInjection;

namespace BinusSchool.Scheduling.FnMovingStudent.Queue
{
    public class ENSNotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ENSNotificationHandler> _logger;

        public ENSNotificationHandler(IServiceProvider serviceProvider, ILogger<ENSNotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(ENSNotification))]
        public Task ENSNotification([QueueTrigger("notification-ens")] string queueItem, CancellationToken cancellationToken)
        {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));

            var handler = notification.IdScenario switch
            {
                "ENS8" => _serviceProvider.GetService<ENS8Notification>(),
                _ => default(IFunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");

            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
