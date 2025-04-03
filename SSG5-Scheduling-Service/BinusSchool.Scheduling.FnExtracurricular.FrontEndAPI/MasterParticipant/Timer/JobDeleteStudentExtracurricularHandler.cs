using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Scheduling.FnExtracurricular.MasterParticipant.Notification;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterParticipant.Timer
{
    public class JobDeleteStudentExtracurricularHandler
    {
#if DEBUG
        private const string _ncrontabExperssion = "0 */5 * * * *"; // once every 5 minute 
#else
        private const string _ncrontabExperssion = "0 0 */1 * * *"; // once every 1 hour 
#endif

        private readonly ILogger<JobDeleteStudentExtracurricularHandler> _logger;
        private readonly IMachineDateTime _dateTime;
        private readonly IServiceProvider _provider;

        public JobDeleteStudentExtracurricularHandler(
            ILogger<JobDeleteStudentExtracurricularHandler> logger,
            IMachineDateTime dateTime,
            IServiceProvider provider)
        {
            _logger = logger;
            _dateTime = dateTime;
            _provider = provider;
        }

        [FunctionName(nameof(JobDeleteStudentExtracurricular))]
        public async Task JobDeleteStudentExtracurricular([TimerTrigger(_ncrontabExperssion)] TimerInfo timerInfo, CancellationToken cancellationToken)
        {
            var schools = SchoolConstant.IdSchools;
            var tasks = new Task[schools.Count];

            // job will run every 4 hours in each schools
            int delayHourBetweenJobEachSchool = 4;

            for (var i = 0; i < schools.Count; i++)
            {
                if ((_dateTime.ServerTime.Hour % delayHourBetweenJobEachSchool) == (i % delayHourBetweenJobEachSchool))
                {
                    _logger.LogInformation("[Timer] Motification scenario {0} for school {1} is running", nameof(JobDeleteStudentExtracurricular), schools[i]);
                    var handler = _provider.GetService<JobDeleteStudentExtracurricularNotification>();
                    tasks[i] = handler.Execute(schools[i], null, null, cancellationToken);
                }
                else
                    tasks[i] = Task.CompletedTask;
            }

            await Task.WhenAll(tasks);

            //var tasks = new Task[0];

            //_logger.LogInformation("[Timer] Motification scenario {0} for school {1} is running", nameof(JobCreateStudentExtracurricularInvoice), "0");
            //var handler = _provider.GetService<JobCreateStudentExtracurricularInvoiceNotification>();
            //tasks[0] = handler.Execute("0", null, null, cancellationToken);

            //await Task.WhenAll(tasks);
        }
    }
}
