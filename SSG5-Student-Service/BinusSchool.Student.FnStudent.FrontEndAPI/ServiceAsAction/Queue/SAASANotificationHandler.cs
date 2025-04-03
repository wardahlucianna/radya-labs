using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using BinusSchool.Student.FnStudent.ServiceAsAction.Notification;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.Student.FnStudent.ServiceAsAction.Queue
{
    public class SAASANotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SAASANotificationHandler> _logger;

        public SAASANotificationHandler(IServiceProvider serviceProvider, ILogger<SAASANotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(SAASANotification))]
        public Task SAASANotification([QueueTrigger("notification-sas")] string queueItem, CancellationToken cancellationToken)
        {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));

            var handler = notification.IdScenario switch
            {
                "SAASA1" => _serviceProvider.GetService<SAASA1Notification>(),
                "SAASA2" => _serviceProvider.GetService<SAASA2Notification>(),
                "SAASA3" => _serviceProvider.GetService<SAASA3Notification>(),
                //"SAASA4" => _serviceProvider.GetService<SAASA4Notification>(),
                _ => default(IFunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");

            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
