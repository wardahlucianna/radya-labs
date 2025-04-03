using System;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.GeneratedSchedule.Validator;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnSchedule.GenerateSchedule
{
    public class StartGeneratedScheduleProcessHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public StartGeneratedScheduleProcessHandler(ISchedulingDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<StartGeneratedScheduleProcessRequest, StartGeneratedScheduleProcessValidator>();

            foreach (var grade in body.Grades)
            {
                if (await _dbContext.Entity<TrGeneratedScheduleProcess>()
                                    .AnyAsync(x => x.IdSchool == body.IdSchool
                                                   && x.Grades.Contains(grade)
                                                   && !x.FinishedAt.HasValue
                                                   && x.Version == body.Version))
                    throw new BadRequestException($"Cannot start generate schedule because there are another job with same grade is running");
            }

            var idProcess = Guid.NewGuid().ToString();
            _dbContext.Entity<TrGeneratedScheduleProcess>().Add(new TrGeneratedScheduleProcess
            {
                Id = idProcess,
                IdSchool = body.IdSchool,
                Grades = JsonConvert.SerializeObject(body.Grades),
                StartAt = _dateTime.ServerTime,
                Version = body.Version
            });
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2(idProcess as object);
        }
    }
}
