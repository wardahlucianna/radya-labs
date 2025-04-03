using System;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.School.FnSchool.TextbookPreparationApprovalSetting.Notification;
using BinusSchool.Common.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BinusSchool.School.FnSchool.TextbookPreparationUserPeriod.Notification;
using BinusSchool.School.FnSchool.TextbookPreparation.Notification;
using BinusSchool.School.FnSchool.TextbookPreparationApproval.Notification;

namespace BinusSchool.School.FnSchool.Queue
{
    public class TPNotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TPNotificationHandler> _logger;

        public TPNotificationHandler(IServiceProvider serviceProvider, ILogger<TPNotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(TPNotificationHandler))]
        public Task TPNotification([QueueTrigger("notification-tp")] string queueItem, CancellationToken cancellationToken)
        {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));

            var handler = notification.IdScenario switch
            {
                "TP1" => _serviceProvider.GetService<TP1Notification>(),
                "TP2" => _serviceProvider.GetService<TP2Notification>(),
                "TP3" => _serviceProvider.GetService<TP3Notification>(),
                "TP4" => _serviceProvider.GetService<TP4Notification>(),
                "TP5" => _serviceProvider.GetService<TP5Notification>(),
                "TP6" => _serviceProvider.GetService<TP6Notification>(),
                "TP7" => _serviceProvider.GetService<TP7Notification>(),
                "TP8" => _serviceProvider.GetService<TP8Notification>(),
                "TP9" => _serviceProvider.GetService<TP9Notification>(),
                "TP10" => _serviceProvider.GetService<TP10Notification>(),
                "TP11" => _serviceProvider.GetService<TP11Notification>(),
                _ => default(IFunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");

            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
