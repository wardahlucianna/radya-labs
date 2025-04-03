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
    public class ATD8Handler
    {
        private readonly ILogger<ATD8Handler> _logger;
        private readonly IMachineDateTime _dateTime;
        private readonly IServiceProvider _provider;

        public ATD8Handler(
            ILogger<ATD8Handler> logger,
            IMachineDateTime dateTime,
            IServiceProvider provider)
        {
            _logger = logger;
            _dateTime = dateTime;
            _provider = provider;
        }

        [FunctionName(nameof(ATD8))]
        public async Task ATD8([TimerTrigger(AttendanceTimeConstant.ATD8ConstantTime
#if DEBUG
                //, RunOnStartup = true
#endif
            )]
            TimerInfo myTimer,
            CancellationToken cancellationToken)
        {
            var schools = SchoolConstant.IdSchools;

            for (var i = 0; i < schools.Count; i++)
            {
                    _logger.LogInformation("[Timer] Motification scenario {0} for school {1} is running", nameof(ATD8), schools[i]);
                    var handlerV2 = _provider.GetService<ATD8V2Notification>();
                    await handlerV2.Execute(schools[i], null, null, cancellationToken);
            }
        }
    }
}
