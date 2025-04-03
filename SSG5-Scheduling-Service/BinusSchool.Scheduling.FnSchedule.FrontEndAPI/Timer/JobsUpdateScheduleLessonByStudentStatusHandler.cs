//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using BinusSchool.Scheduling.FnSchedule.Schedule;
//using BinusSchool.Common.Abstractions;
//using BinusSchool.Common.Constants;
//using BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using BinusSchool.Scheduling.FnSchedule.GenerateSchedule;

//namespace BinusSchool.Scheduling.FnSchedule.Timer
//{
//    public class JobsUpdateScheduleLessonByStudentStatusHandler
//    {
//#if DEBUG
//        // private const string _ncrontabExperssion = "0 */1 * * * *"; // once every 1 minute 
//        private const string _ncrontabExperssion = "0 0 */12 * * *"; // once every 12 hour 
//#else
//        private const string _ncrontabExperssion = "0 0 */12 * * *"; // once every 12 hour 
//#endif
//        private readonly ILogger<JobsUpdateScheduleLessonByStudentStatusHandler> _logger;
//        private readonly IMachineDateTime _dateTime;
//        private readonly IServiceProvider _provider;
//        private readonly UpdateGenerateScheduleLessonByStudentStatusHandler _updateGenerateScheduleLessonByStudentStatusHandler;

//        public JobsUpdateScheduleLessonByStudentStatusHandler(
//            ILogger<JobsUpdateScheduleLessonByStudentStatusHandler> logger,
//            IMachineDateTime dateTime,
//            IServiceProvider provider,
//            UpdateGenerateScheduleLessonByStudentStatusHandler UpdateGenerateScheduleLessonByStudentStatusHandler)
//        {
//            _logger = logger;
//            _dateTime = dateTime;
//            _provider = provider;
//            _updateGenerateScheduleLessonByStudentStatusHandler = UpdateGenerateScheduleLessonByStudentStatusHandler;
//        }

//        [FunctionName(nameof(JobsUpdateScheduleLessonByStudentStatus))]
//        public async Task JobsUpdateScheduleLessonByStudentStatus([TimerTrigger(_ncrontabExperssion)] TimerInfo timerInfo, CancellationToken cancellationToken)
//        {
//            var schools = SchoolConstant.IdSchools;
//            var tasks = new Task[schools.Count];

//            // get schedule for notification
//            var schedules = SchoolConstant.IdSchools.ToDictionary(x => x, x => 3);


//            for (var i = 0; i < schools.Count; i++)
//            {
//                if (_dateTime.ServerTime.Hour == schedules[schools[i]])
//                {
//                    _logger.LogInformation("[Timer] Update generate schedule lesson by student status {0} for school {1} is running", nameof(JobsUpdateScheduleLessonByStudentStatus), schools[i]);
//                    var handler = _provider.GetService<UpdateGenerateScheduleLessonByStudentStatusHandler>();
//                    tasks[i] = handler.UpdateGenerate();
//                }
//                else
//                    tasks[i] = Task.CompletedTask;
//            }
//            await Task.WhenAll(tasks);
//        }
//    }
//}
