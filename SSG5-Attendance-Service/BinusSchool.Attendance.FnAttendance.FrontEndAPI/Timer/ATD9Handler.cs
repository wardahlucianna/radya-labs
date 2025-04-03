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
    public class ATD9Handler
    {
        private readonly ILogger<ATD9Handler> _logger;
        private readonly IMachineDateTime _dateTime;
        private readonly IServiceProvider _provider;

        public ATD9Handler(
            ILogger<ATD9Handler> logger,
            IMachineDateTime dateTime,
            IServiceProvider provider)
        {
            _logger = logger;
            _dateTime = dateTime;
            _provider = provider;
        }

        [FunctionName(nameof(ATD9))]
        public async Task ATD9([TimerTrigger(AttendanceTimeConstant.ATD9ConstantTime
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
                    _logger.LogInformation("[Timer] Motification scenario {0} for school {1} is running", nameof(ATD9), schools[i]);
                    var handlerV2 = _provider.GetService<ATD9V2Notification>();
                    await handlerV2.Execute(schools[i], null, null, cancellationToken);
            }
        }
    }
}
