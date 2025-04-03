using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Teaching.FnLessonPlan.LessonPlan.Notification;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan.Timer
{
    public class LP1Handler
    {
#if DEBUG
        private const string _ncrontabExperssion = "0 */2 * * * *"; // once every 2 minute 
#else
        private const string _ncrontabExperssion = "0 0 */1 * * *"; // once every 1 hour 
        //private const string _ncrontabExperssion = "0 */30 * * * *"; // once every 30 minute 
#endif

        private readonly ILogger<LP1Handler> _logger;
        private readonly IMachineDateTime _dateTime;
        private readonly IServiceProvider _provider;

        public LP1Handler(ILogger<LP1Handler> logger,
            IMachineDateTime dateTime,
            IServiceProvider provider)
        {
            _logger = logger;
            _dateTime = dateTime;
            _provider = provider;
        }

        [FunctionName(nameof(LP1))]
        public async Task LP1([TimerTrigger(_ncrontabExperssion)] TimerInfo timerInfo, CancellationToken cancellationToken)
        {
            var schools = SchoolConstant.IdSchools;

#if DEBUG
            var tasks = new Task[1];
#else
            var tasks = new Task[schools.Count];
            // get schedule for notification
            var daySchedules = DayOfWeek.Monday;
            var schedules = SchoolConstant.IdSchools.ToDictionary(x => x, x => 12);

#endif

#if DEBUG
            _logger.LogInformation("[Timer] Scheduled notification scenario {0} for school {1}.", nameof(LP1), string.Join(", ", schools[2]));
            //var handler = _provider.GetService<LP1Notification>();
            //tasks[0] = handler.Execute(schools[1], null, null, cancellationToken);//cukup satu sekolah
            await Task.WhenAll(tasks);
#else
            for (var i = 0; i < schools.Count; i++)
            {
                if (daySchedules == _dateTime.ServerTime.DayOfWeek && _dateTime.ServerTime.Hour == schedules[schools[i]])
                {
                    _logger.LogInformation("[Timer] Scheduled notification scenario {0} for school {1}.", nameof(LP1), string.Join(", ", schools[i]));
                    var handler = _provider.GetService<LP1Notification>();
                    tasks[i] = handler.Execute(schools[i], null, null, cancellationToken);
                }
                else
                    tasks[i] = Task.CompletedTask;

            }
            //for (var i = 0; i < schools.Count; i++)
            //{
            //    _logger.LogInformation("[Timer] Scheduled notification scenario {0} for school {1}.", nameof(LP1), string.Join(", ", schools[i]));
            //    var handler = _provider.GetService<LP1Notification>();
            //    tasks[i] = handler.Execute(schools[i], null, null, cancellationToken);
            //}
            await Task.WhenAll(tasks);
#endif

        }
    }
}
