using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Attendance.FnAttendance.AttendanceAdministration.Notification;
using BinusSchool.Attendance.FnAttendance.AttendanceEntry.Notification;
using BinusSchool.Attendance.FnAttendance.AttendanceV2.Notification;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;
using BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm.Notification;
using Microsoft.Extensions.DependencyInjection;

namespace BinusSchool.Attendance.FnAttendance.Queue
{
    public class EHNNotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ATDNotificationHandler> _logger;

        public EHNNotificationHandler(IServiceProvider serviceProvider, ILogger<ATDNotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(EHNNotification))]
        public Task EHNNotification([QueueTrigger("notification-ehn")] string queueItem, CancellationToken cancellationToken)
        {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));

            var handler = notification.IdScenario switch
            {
                "EHN9" => _serviceProvider.GetService<EHN9Notification>(),
                "EHN10" => _serviceProvider.GetService<EHN10Notification>(),
                _ => default(IFunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");

            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
