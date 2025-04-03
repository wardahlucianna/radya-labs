using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using BinusSchool.Student.FnStudent.MeritDemeritTeacher;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BinusSchool.Common.Model;
using Newtonsoft.Json;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Student.FnStudent.StudentExitForm.Notification;

namespace BinusSchool.Student.FnStudent.Queue
{
    public class SXTNotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SXTNotificationHandler> _logger;

        public SXTNotificationHandler(IServiceProvider serviceProvider, ILogger<SXTNotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(STDENotification))]
        public Task STDENotification([QueueTrigger("notification-student-exit")] string queueItem, CancellationToken cancellationToken)
        {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));

            var handler = notification.IdScenario switch
            {
                "SXT1" => _serviceProvider.GetService<SXT1Notification>(),
                "SXT2" => _serviceProvider.GetService<SXT2Notification>(),
                "SXT3" => _serviceProvider.GetService<SXT3Notification>(),
                "SXT4" => _serviceProvider.GetService<SXT4Notification>(),
                "SXT5" => _serviceProvider.GetService<SXT5Notification>(),
                _ => default(IFunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");

            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
