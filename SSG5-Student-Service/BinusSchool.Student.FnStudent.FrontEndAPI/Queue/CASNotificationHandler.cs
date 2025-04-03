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
using BinusSchool.Student.FnStudent.CreativityActivityService;

namespace BinusSchool.Student.FnStudent.EmergencyAttendance.Queue
{
    public class CASNotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CASNotificationHandler> _logger;

        public CASNotificationHandler(IServiceProvider serviceProvider, ILogger<CASNotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(CASNotification))]
        public Task CASNotification([QueueTrigger("notification-cas")] string queueItem, CancellationToken cancellationToken)
        {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));

            var handler = notification.IdScenario switch
            {
                "CAS1" => _serviceProvider.GetService<CAS1Notification>(),
                "CAS2" => _serviceProvider.GetService<CAS2Notification>(),
                "CAS3" => _serviceProvider.GetService<CAS3Notification>(),
                "CAS4" => _serviceProvider.GetService<CAS4Notification>(),
                "CAS5" => _serviceProvider.GetService<CAS5Notification>(),
                "CAS7" => _serviceProvider.GetService<CAS7Notification>(),
                "CAS8" => _serviceProvider.GetService<CAS8Notification>(),
                "CAS14" => _serviceProvider.GetService<CAS14Notification>(),
                _ => default(FunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");

            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
