using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnAscTimetable.AscTimetable
{
    public class CheckAscTimetableProcessHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public CheckAscTimetableProcessHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<StartAscTimetableProcessRequest>(nameof(StartAscTimetableProcessRequest.IdSchool),
                                                                                nameof(StartAscTimetableProcessRequest.Grades));

            foreach (var grade in param.Grades)
            {
                if (await _dbContext.Entity<TrAscTimetableProcess>()
                                    .AnyAsync(x => x.IdSchool == param.IdSchool
                                                   && x.Grades.Contains(grade)
                                                   && !x.FinishedAt.HasValue))
                    throw new BadRequestException($"Cannot start upload asc because there are another job with same grade is running");
            }

            return Request.CreateApiResult2(null);
        }
    }
}
