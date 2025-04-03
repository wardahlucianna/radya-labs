using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Newtonsoft.Json;
using Google.Apis.Logging;
using Microsoft.Extensions.Logging;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Student.FnStudent.StudentExitForm.Notification;
using Microsoft.Extensions.DependencyInjection;
using BinusSchool.Student.FnStudent.Achievement.Notification;

namespace BinusSchool.Student.FnStudent.Queue
{
    public class SANotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SANotificationHandler> _logger;

        public SANotificationHandler(IServiceProvider serviceProvider, ILogger<SANotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(SANotification))]
        public Task SANotification([QueueTrigger("notification-student-achievement")] string queueItem, CancellationToken cancellationToken)
        {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));

            var handler = notification.IdScenario switch
            {
                "SA1" => _serviceProvider.GetService<SA1Notification>(),
                "SA2" => _serviceProvider.GetService<SA2Notification>(),
                "SA3" => _serviceProvider.GetService<SA3Notification>(),
                "SA4" => _serviceProvider.GetService<SA4Notification>(),
                _ => default(IFunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");

            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
