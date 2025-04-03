using System;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using BinusSchool.User.FnUser.User.Notification;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.User.FnUser.Queue
{
    public class USRNotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<USRNotificationHandler> _logger;

        public USRNotificationHandler(IServiceProvider serviceProvider, ILogger<USRNotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(USRNotification))]
        public Task USRNotification([QueueTrigger("notification-usr-user")] string queueItem, CancellationToken cancellationToken)
        {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));
            
            var handler = notification.IdScenario switch
            {
                "USR1" => _serviceProvider.GetService<USR1Notification>(),
                "USR5" => _serviceProvider.GetService<USR5Notification>(),
                "USR9" => _serviceProvider.GetService<USR9Notification>(),
                "USR10" => _serviceProvider.GetService<USR10Notification>(),
                _ => default(IFunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");
            
            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
