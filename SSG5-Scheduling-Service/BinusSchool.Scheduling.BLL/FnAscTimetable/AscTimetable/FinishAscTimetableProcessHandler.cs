using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnAscTimetable.AscTimetable.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnAscTimetable.AscTimetable
{
    public class FinishAscTimetableProcessHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public FinishAscTimetableProcessHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<FinishAscTimetableProcessRequest, FinishAscTimetableProcessValidator>();

            var process = await _dbContext.Entity<TrAscTimetableProcess>()
                                          .Where(x => x.Id == body.IdProcess)
                                          .SingleOrDefaultAsync();
            if (process is null)
                throw new NotFoundException("process is not found");

            process.FinishedAt = DateTime.Now;
            _dbContext.Entity<TrAscTimetableProcess>().Update(process);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2(null);
        }
    }
}
