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
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnStudent.MeritDemeritTeacher;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Student.FnStudent.Timer
{
    public class MeritDemeritPointHandler
    {
        private readonly ILogger<MeritDemeritPointHandler> _logger;
        private readonly IMachineDateTime _dateTime;
        private readonly IStudentDbContext _dbContext;
        private readonly IMeritDemeritTeacher _meritDemeritTeacherApi;
        public MeritDemeritPointHandler(ILogger<MeritDemeritPointHandler> logger,
            IMachineDateTime dateTime,
            IMeritDemeritTeacher MeritDemeritTeacherApi,
            IStudentDbContext DbContext)
        {
            _logger = logger;
            _dateTime = dateTime;
            _meritDemeritTeacherApi = MeritDemeritTeacherApi;
            _dbContext = DbContext;
        }

        [FunctionName(nameof(MeritDemeritPoint))]
        public async Task MeritDemeritPoint([TimerTrigger(StudentTimeConstant.MeritDemeritPointConstantTime
#if DEBUG
                //, RunOnStartup = true
#endif
            )]
            TimerInfo myTimer,
            CancellationToken cancellationToken)
        {
            var schools = SchoolConstant.IdSchools;
            var tasks = new Task[schools.Count];
            // get schedule for notification
            _logger.LogInformation("time merit demerit reset", _dateTime.ServerTime);

            for (var i = 0; i < schools.Count; i++)
            {
                await _meritDemeritTeacherApi.CalculateMeritDemeritPointSmt1(new CalculateMeritDemeritPointRequest
                {
                    IdSchool = schools[i],
                });

                await _meritDemeritTeacherApi.CalculateMeritDemeritPoint(new CalculateMeritDemeritPointRequest
                {
                    IdSchool = schools[i],
                });
                tasks[i] = Task.CompletedTask;
            }
            await Task.WhenAll(tasks);
        }
    }
}
