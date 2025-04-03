using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceV2.Notification;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Attendance.FnAttendance.Timer
{
    public class ATD7Handler
    {
        private readonly ILogger<ATD7Handler> _logger;
        private readonly IMachineDateTime _dateTime;
        private readonly IServiceProvider _provider;

        public ATD7Handler(
            ILogger<ATD7Handler> logger,
            IMachineDateTime dateTime,
            IServiceProvider provider)
        {
            _logger = logger;
            _dateTime = dateTime;
            _provider = provider;
        }

        [FunctionName(nameof(ATD7))]
        public async Task ATD7([TimerTrigger(AttendanceTimeConstant.ATD7ConstantTime
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
                        _logger.LogInformation("[Timer] Motification scenario {0} for school {1} is running", nameof(ATD7), schools[i]);
                        var handlerV2 = scope.ServiceProvider.GetService<ATD7V2Notification>();
                        await handlerV2.Execute(schools[i], null, null, cancellationToken);
                }
            }
        }
    }
}
