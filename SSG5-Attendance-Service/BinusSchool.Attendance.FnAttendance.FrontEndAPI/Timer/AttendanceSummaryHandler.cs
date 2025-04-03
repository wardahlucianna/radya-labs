//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using BinusSchool.Attendance.FnAttendance.AttendanceSummary;
//using BinusSchool.Common.Abstractions;
//using BinusSchool.Common.Constants;
//using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;

//namespace BinusSchool.Attendance.FnAttendance.Timer
//{
//    public class AttendanceSummaryHandler
//    {
//#if DEBUG
//        private const string _ncrontabExperssion = "0 0 */1 * * *"; // once every 2 minute 
//#else
//        private const string _ncrontabExperssion = "0 0 */1 * * *"; // once every 1 hour 
//#endif
//        private readonly ILogger<AttendanceSummaryHandler> _logger;
//        private readonly IMachineDateTime _dateTime;
//        private readonly IServiceProvider _provider;
//        private readonly AttendanceSummaryCalculationHandler _attendanceSummaryCalculationHandler;

//        public AttendanceSummaryHandler(
//            ILogger<AttendanceSummaryHandler> logger,
//            IMachineDateTime dateTime,
//            IServiceProvider provider,
//            AttendanceSummaryCalculationHandler attendanceSummaryCalculationHandler)
//        {
//            _logger = logger;
//            _dateTime = dateTime;
//            _provider = provider;
//            _attendanceSummaryCalculationHandler = attendanceSummaryCalculationHandler;
//        }

//        [FunctionName(nameof(AttendanceSummary))]
//        public async Task AttendanceSummary([TimerTrigger(_ncrontabExperssion)] TimerInfo timerInfo, CancellationToken cancellationToken)
//        {
//            var schools = SchoolConstant.IdSchools;
//            var tasks = new Task[schools.Count];

//            // get schedule for notification
//            var schedules = SchoolConstant.IdSchools.ToDictionary(x => x, x => 3);


//            for (var i = 0; i < schools.Count; i++)
//            {
//                if (_dateTime.ServerTime.Hour == schedules[schools[i]])
//                {
//                    _logger.LogInformation("[Timer] Attendance Summary Calculation {0} for school {1} is running", nameof(AttendanceSummary), schools[i]);
//                    //tasks[i] = _attendanceSummaryCalculationHandler.Calculate(new AttendanceSummaryCalculationRequest { IdSchool = schools[i], Date = _dateTime.ServerTime.AddDays(-1) });
//                    var handler = _provider.GetService<AttendanceSummaryCalculationHandler>();
//                    //tasks[i] = handler.Calculate(new AttendanceSummaryCalculationRequest { IdSchool = schools[i], Date = _dateTime.ServerTime.AddDays(-1) });
//                    tasks[i] = handler.Calculate(new AttendanceSummaryCalculationRequest { IdSchool = schools[i], Date = _dateTime.ServerTime });
//                }
//                else
//                    tasks[i] = Task.CompletedTask;
//            }
//            //var tasks1 = new Task[1];
//            //_logger.LogInformation("[Timer] Attendance Summary Calculation {0} for school {1} is running", nameof(AttendanceSummary), schools[1]);
//            //var handler = _provider.GetService<AttendanceSummaryCalculationHandler>();
//            //tasks1[0] = handler.Calculate(new AttendanceSummaryCalculationRequest { IdSchool = schools[1], Date = _dateTime.ServerTime });
//            await Task.WhenAll(tasks);
//        }
//    }
//}
