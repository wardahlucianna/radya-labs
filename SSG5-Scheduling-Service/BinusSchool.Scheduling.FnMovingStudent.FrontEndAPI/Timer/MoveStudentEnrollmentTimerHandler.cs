using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Data.Api.Scheduling.FnMovingStudent;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Scheduling.FnMovingStudent.Timer
{
    public class MoveStudentEnrollmentTimerHandler
    {
        private readonly ILogger<MoveStudentEnrollmentTimerHandler> _logger;
        private readonly IMachineDateTime _dateTime;
        private readonly IMoveStudentEnrollment _moveStudentEnrollment;

        public MoveStudentEnrollmentTimerHandler(
            ILogger<MoveStudentEnrollmentTimerHandler> logger,
            IMachineDateTime dateTime,
            IMoveStudentEnrollment MoveStudentEnrollment
            )
        {
            _logger = logger;
            _dateTime = dateTime;
            _moveStudentEnrollment = MoveStudentEnrollment;
        }

        [FunctionName(nameof(MoveStudentEnrollment))]
        public async Task MoveStudentEnrollment([TimerTrigger(MovingStudentTimeConstant.MoveStudentEnrollmentConstantTime
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
                _logger.LogInformation("[Timer] Move Student Enrollment for school {0} is running", schools[i]);

                var request = new MoveStudentEnrollmentSyncRequest
                {
                    idSchool = schools[i],
                    Date = _dateTime.ServerTime.Date,
                };

                await _moveStudentEnrollment.MoveStudentEnrollmentSync(request);
            }
        }
    }
}
