using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceEntry.Notification;
using BinusSchool.Attendance.FnAttendance.AttendanceV2.Notification;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Attendance.FnAttendance.Timer
{
    public class ATD6Handler
    {
        private readonly ILogger<ATD6Handler> _logger;
        private readonly IMachineDateTime _dateTime;
        private readonly IServiceProvider _provider;

        public ATD6Handler(
            ILogger<ATD6Handler> logger,
            IMachineDateTime dateTime,
            IServiceProvider provider)
        {
            _logger = logger;
            _dateTime = dateTime;
            _provider = provider;
        }

        [FunctionName(nameof(ATD6))]
        public async Task ATD6([TimerTrigger(AttendanceTimeConstant.ATD6ConstantTime
#if DEBUG
                //, RunOnStartup = true
#endif
            )]
            TimerInfo myTimer,
            CancellationToken cancellationToken)
        {
            var schools = SchoolConstant.IdSchools;

            using (var scope = _provider.CreateScope())
            {
                for (var i = 0; i < schools.Count; i++)
                {
                        _logger.LogInformation("[Timer] Motification scenario {0} for school {1} is running", nameof(ATD6), schools[i]);
                        var handlerV2 = scope.ServiceProvider.GetService<ATD6V2Notification>();
                        await handlerV2.Execute(schools[i], null, null, cancellationToken);
                }
            }
        }
    }
}
