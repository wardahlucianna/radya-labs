using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Student.FnStudent.Timer
{
    public class MeritDemeritPointV2Handler
    {
        private const string _ncrontabExperssion = "0 0 */1 * * *"; // once every 1 hour 

        private readonly ILogger<MeritDemeritPointV2Handler> _logger;
        private readonly IMachineDateTime _dateTime;
        private readonly IMeritDemeritTeacher _meritDemeritTeacherApi;

        public MeritDemeritPointV2Handler(
            ILogger<MeritDemeritPointV2Handler> logger,
            IMachineDateTime dateTime,
            IMeritDemeritTeacher meritDemeritTeacherApi)
        {
            _logger = logger;
            _dateTime = dateTime;
            _meritDemeritTeacherApi = meritDemeritTeacherApi;
        }

        [FunctionName(nameof(MeritDemeritPointV2))]
        public async Task MeritDemeritPointV2([TimerTrigger(_ncrontabExperssion)] TimerInfo timerInfo, CancellationToken cancellationToken)
        {
            var schools = SchoolConstant.IdSchools;
            var tasks = new Task[schools.Count];

            // get schedule for notification
            var schedules = SchoolConstant.IdSchools.ToDictionary(x => x, x => 1);


            for (var i = 0; i < schools.Count; i++)
            {
                if (_dateTime.ServerTime.Hour == schedules[schools[i]])
                {
                    ResetMeritDemeritPointV2Request param = new ResetMeritDemeritPointV2Request
                    {
                        IdSchool = schools[i],
                    };

                    var ResetMeritDemeritPointV2 = await _meritDemeritTeacherApi.ResetMeritDemeritPointV2(param);
                }
                else
                    tasks[i] = Task.CompletedTask;
            }

            await Task.WhenAll(tasks);
        }
    }
}
