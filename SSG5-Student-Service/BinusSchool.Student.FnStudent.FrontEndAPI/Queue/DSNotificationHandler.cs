
using System;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class DSNotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DSNotificationHandler> _logger;

        public DSNotificationHandler(IServiceProvider serviceProvider, ILogger<DSNotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(DSNotification))]
        public Task DSNotification([QueueTrigger("notification-ds-student")] string queueItem, CancellationToken cancellationToken)
        {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));
            var handler = notification.IdScenario switch
            {
                "DS1" => _serviceProvider.GetService<DS1Notification>(),
                "DS2" => _serviceProvider.GetService<DS2Notification>(),
                "DS3" => _serviceProvider.GetService<DS3Notification>(),
                "DS4" => _serviceProvider.GetService<DS4Notification>(),
                "DS5" => _serviceProvider.GetService<DS5Notification>(),
                "DS6" => _serviceProvider.GetService<DS6Notification>(),
                "DS7" => _serviceProvider.GetService<DS7Notification>(),
                _ => default(IFunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");

            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);

        }
    }
}
