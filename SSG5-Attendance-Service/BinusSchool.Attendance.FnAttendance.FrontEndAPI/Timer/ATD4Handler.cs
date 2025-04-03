using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceSummary.Notification;
using BinusSchool.Attendance.FnAttendance.Timer;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Functions.Abstractions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary.Timer
{
    public class ATD4Handler
    {
        private readonly ILogger<ATD4Handler> _logger;
        private readonly IMachineDateTime _dateTime;
        private readonly IServiceProvider _provider;

        public ATD4Handler(
            ILogger<ATD4Handler> logger,
            IMachineDateTime dateTime, 
            IServiceProvider provider)
        {
            _logger = logger;
            _dateTime = dateTime;
            _provider = provider;
        }

        [FunctionName(nameof(ATD4))]
        public async Task ATD4([TimerTrigger(AttendanceTimeConstant.ATD4ConstantTime
#if DEBUG
                //, RunOnStartup = true
#endif
            )]
            TimerInfo myTimer,
            CancellationToken cancellationToken)
        {
            var schools = SchoolConstant.IdSchools;
            var tasks = new Task[schools.Count];

            for (var i = 0; i < schools.Count; i++)
            {
                _logger.LogInformation("[Timer] Motification scenario {0} for school {1} is running", nameof(ATD4), schools[i]);
                var handler = _provider.GetService<ATD4Notification>();
                tasks[i] = handler.Execute(schools[i], null, null, cancellationToken);
            }

            await Task.WhenAll(tasks);
        }
    }
}
