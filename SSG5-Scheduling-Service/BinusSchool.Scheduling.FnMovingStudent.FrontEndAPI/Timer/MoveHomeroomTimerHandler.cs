using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Data.Api.Scheduling.FnMovingStudent;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MovingStudentHomeroom;

namespace BinusSchool.Scheduling.FnMovingStudent.Timer
{
    public class MoveHomeroomTimerHandler
    {
        private readonly ILogger<MoveHomeroomTimerHandler> _logger;
        private readonly IMachineDateTime _dateTime;
        private readonly IMoveStudentHomeroom _moveStudentHomeroom;

        public MoveHomeroomTimerHandler(
            ILogger<MoveHomeroomTimerHandler> logger,
            IMachineDateTime dateTime,
            IMoveStudentHomeroom moveStudentHomeroom
            )
        {
            _logger = logger;
            _dateTime = dateTime;
            _moveStudentHomeroom = moveStudentHomeroom;
        }

        [FunctionName(nameof(MoveHomeroom))]
        public async Task MoveHomeroom([TimerTrigger(MovingStudentTimeConstant.MoveHomeroomConstantTime
#if DEBUG
                , RunOnStartup = true
#endif
            )]
            TimerInfo myTimer,
            CancellationToken cancellationToken)
        {
            var schools = SchoolConstant.IdSchools;

            for (var i = 0; i < schools.Count; i++)
            {
                _logger.LogInformation("[Timer] Move Homeroom for school {0} is running", schools[i]);

                var request = new MoveHomeroomSyncRequest
                {
                    IdSchool = schools[i],
                    Date = _dateTime.ServerTime.Date,
                };

                await _moveStudentHomeroom.MoveHomeroomSync(request);
            }
        }
    }
}
