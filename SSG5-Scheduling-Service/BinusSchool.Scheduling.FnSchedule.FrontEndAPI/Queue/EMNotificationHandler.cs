using System.Threading.Tasks;
using System;
using System.Threading;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using BinusSchool.Scheduling.FnSchedule.SchoolEvent.Notification;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BinusSchool.Scheduling.FnSchedule.GenerateSchedule;

namespace BinusSchool.Scheduling.FnSchedule.Queue
{
    public class EMNotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EMNotificationHandler> _logger;

        public EMNotificationHandler(IServiceProvider serviceProvider, ILogger<EMNotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(EMNotification))]
        public Task EMNotification([QueueTrigger("notification-em-schedule")] string queueItem, CancellationToken cancellationToken)
         {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));

            var handler = notification.IdScenario switch
            {
                "EM1" => _serviceProvider.GetService<EM1Notification>(),
                "EM2" => _serviceProvider.GetService<EM2Notification>(),
                "EM3" => _serviceProvider.GetService<EM3Notification>(),
                "EM4" => _serviceProvider.GetService<EM4Notification>(),
                "EM5" => _serviceProvider.GetService<EM5Notification>(),
                "EM6" => _serviceProvider.GetService<EM6Notification>(),
                "EM7" => _serviceProvider.GetService<EM7Notification>(),
                "EM8" => _serviceProvider.GetService<EM8Notification>(),
                "EM9" => _serviceProvider.GetService<EM9Notification>(),
                "EM10" => _serviceProvider.GetService<EM10Notification>(),
                "EM11" => _serviceProvider.GetService<EM11Notification>(),
                "EM12" => _serviceProvider.GetService<EM12Notification>(),
                "EM13" => _serviceProvider.GetService<EM13Notification>(),
                _ => default(IFunctionsNotificationHandler)
            };

            if(notification.IdScenario == "UGBE")
            {
                var handlerUpdateGenerate = _serviceProvider.GetService<UpdateGenerateScheduleLessonByEventHandler>();

                var dataKeyValue = JsonConvert.DeserializeObject<CodeWithIdVm>(JsonConvert.SerializeObject(notification.KeyValues["JobsEventUpdateGenerate"]));

                _logger.LogInformation("[Queue] Update Generate by Event for Event Id {0} school {1}.", dataKeyValue.Id, notification.IdSchool);

                return handlerUpdateGenerate.UpdateGenerateScheduleLessonByEvent(dataKeyValue.Id,dataKeyValue.Code);
            }

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");

            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
