using System;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using BinusSchool.Scheduling.FnSchedule.EmailInvitation.Notification;
using BinusSchool.Scheduling.FnSchedule.AppointmentBooking;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BinusSchool.Scheduling.FnSchedule.BreakSetting;
using Newtonsoft.Json;
using BinusSchool.Scheduling.FnSchedule.AppointmentBooking.Notification;

namespace BinusSchool.Scheduling.FnSchedule.Queue
{
    public class APPNotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AYSNotificationHandler> _logger;

        public APPNotificationHandler(IServiceProvider serviceProvider, ILogger<AYSNotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(APPNotification))]
        public Task APPNotification([QueueTrigger("notification-app")] string queueItem, CancellationToken cancellationToken)
        {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));
            
            var handler = notification.IdScenario switch
            {
                "APP1" => _serviceProvider.GetService<APP1Notification>(),
                "APP2" => _serviceProvider.GetService<APP2Notification>(),
                "APP8" => _serviceProvider.GetService<APP8Notification>(),
                "APP10" => _serviceProvider.GetService<APP10Notification>(),
                "APP11" => _serviceProvider.GetService<APP11Notification>(),
                "APP12" => _serviceProvider.GetService<APP12Notification>(),
                "APP18" => _serviceProvider.GetService<APP18Notification>(),
                "APP19" => _serviceProvider.GetService<APP19Notification>(),
                "APP20" => _serviceProvider.GetService<APP20Notification>(),
                "APP21" => _serviceProvider.GetService<APP21Notification>(),
                "APP26" => _serviceProvider.GetService<APP26Notification>(),
                "APP27" => _serviceProvider.GetService<APP27Notification>(),
                "APP28" => _serviceProvider.GetService<APP28Notification>(),
                "APP29" => _serviceProvider.GetService<APP29Notification>(),
                "ENS1" => _serviceProvider.GetService<ENS1Notification>(),
                _ => default(IFunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");
            
            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
