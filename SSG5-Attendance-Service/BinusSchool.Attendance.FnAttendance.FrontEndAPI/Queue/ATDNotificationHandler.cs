using System;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceAdministration.Notification;
using BinusSchool.Attendance.FnAttendance.AttendanceAdministrationV2.Notification;
using BinusSchool.Attendance.FnAttendance.AttendanceEntry.Notification;
using BinusSchool.Attendance.FnAttendance.AttendanceV2.Notification;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.Queue
{
    public class ATDNotificationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ATDNotificationHandler> _logger;

        public ATDNotificationHandler(IServiceProvider serviceProvider, ILogger<ATDNotificationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [FunctionName(nameof(ATDNotification))]
        public Task ATDNotification([QueueTrigger("notification-atd-attendance")] string queueItem, CancellationToken cancellationToken)
        {
            var notification = JsonConvert.DeserializeObject<NotificationQueue>(queueItem);
            _logger.LogInformation("[Queue] Dequeue notification scenario {0} for school {1}.", notification.IdScenario, notification.IdSchool);

            if (string.IsNullOrEmpty(notification.IdScenario))
                throw new ArgumentNullException(nameof(notification.IdScenario));

            var handler = notification.IdScenario switch
            {
                "ATD1" => _serviceProvider.GetService<ATD1Notification>(),
                "ATD2" => _serviceProvider.GetService<ATD2Notification>(),
                "ATD5" => _serviceProvider.GetService<ATD5Notification>(),
                "ATD10" => _serviceProvider.GetService<ATD10V2Notification>(),
                "ATD11" => _serviceProvider.GetService<ATD11V2Notification>(),
                "ATD12" => _serviceProvider.GetService<ATD12V2Notification>(),
                "ATD16" => _serviceProvider.GetService<ATD16Notification>(),
                "ATD17" => _serviceProvider.GetService<ATD17Notification>(),
                "ATD18" => _serviceProvider.GetService<ATD18Notification>(),
                "ATD19" => _serviceProvider.GetService<ATD19Notification>(),
                "ATD20" => _serviceProvider.GetService<ATD20Notification>(),
                "ATD21" => _serviceProvider.GetService<ATD21Notification>(),
                "ATD6" => _serviceProvider.GetService<ATD6V2Notification>(),
                "ATD8" => _serviceProvider.GetService<ATD8V2Notification>(),
                "ENS4" => _serviceProvider.GetService<ENS4Notification>(),
                "ATD22" => _serviceProvider.GetService<ATD22Notification>(),
                _ => default(IFunctionsNotificationHandler)
            };

            if (handler is null)
                throw new InvalidOperationException($"Handler for notification scenario {notification.IdScenario} is not found.");

            return handler.Execute(notification.IdSchool, notification.IdRecipients, notification.KeyValues, cancellationToken);
        }
    }
}
