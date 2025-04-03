using System.Threading.Tasks;
using System;
using System.Threading;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BinusSchool.Student.FnStudent.EmergencyAttendance.Notification;
using BinusSchool.Common.Functions.Handler;

namespace BinusSchool.Student.FnStudent.EmergencyAttendance.Queue
{
    public class ATDNotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ATDNotificationHandler> _logger;

        public ATDNotificationHandler(IServiceProvider serviceProvider, ILogger<ATDNotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(ATDNotification))]
        public Task ATDNotification([QueueTrigger("notification-atd-student")] string queueItem, CancellationToken cancellationToken)
        {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));

            var handler = notification.IdScenario switch
            {
                "ATD13" => _serviceProvider.GetService<ATD13Notification>(),
                _ => default(FunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");

            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
