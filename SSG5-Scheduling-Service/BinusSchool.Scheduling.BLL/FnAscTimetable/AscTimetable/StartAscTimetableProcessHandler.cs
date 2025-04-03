using System;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnAscTimetable.AscTimetable.Validator;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnAscTimetable.AscTimetable
{
    public class StartAscTimetableProcessHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        public StartAscTimetableProcessHandler(ISchedulingDbContext dbContext, IMachineDateTime datetime)
        {
            _dbContext = dbContext;
            _datetime = datetime;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<StartAscTimetableProcessRequest, StartAscTimetableProcessValidator>();

            foreach (var grade in body.Grades)
            {
                if (await _dbContext.Entity<TrAscTimetableProcess>()
                                    .AnyAsync(x => x.IdSchool == body.IdSchool
                                                   && x.Grades.Contains(grade)
                                                   && !x.FinishedAt.HasValue))
                    throw new BadRequestException($"Cannot start upload asc because there are another job with same grade is running");
            }

            var idProcess = Guid.NewGuid().ToString();
            _dbContext.Entity<TrAscTimetableProcess>().Add(new TrAscTimetableProcess
            {
                Id = idProcess,
                IdSchool = body.IdSchool,
                Grades = JsonConvert.SerializeObject(body.Grades),
                StartAt = _datetime.ServerTime
            });
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2(idProcess as object);
        }
    }
}
