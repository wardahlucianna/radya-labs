using System.Threading.Tasks;
using System;
using System.Threading;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc.Notification;
using BinusSchool.Common.Functions.Handler;

namespace BinusSchool.Student.FnGuidanceCounseling.Queue
{
    public class GCNotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GCNotificationHandler> _logger;

        public GCNotificationHandler(IServiceProvider serviceProvider, ILogger<GCNotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(GCNotification))]
        public Task GCNotification([QueueTrigger("notification-gc-guidancecounseling")] string queueItem, CancellationToken cancellationToken)
        {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));

            var handler = notification.IdScenario switch
            {
                "GC1" => _serviceProvider.GetService<GC1Notification>(),
                "GC2" => _serviceProvider.GetService<GC2Notification>(),
                "GC3" => _serviceProvider.GetService<GC3Notification>(),
                _ => default(IFunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");

            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
