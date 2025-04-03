using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.GenerateSchedule.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.GenerateSchedule
{
    public class FinishGeneratedScheduleProcessHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public FinishGeneratedScheduleProcessHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<FinishGeneratedScheduleProcessRequest, FinishGeneratedScheduleProcessValidator>();

            var process = await _dbContext.Entity<TrGeneratedScheduleProcess>()
                                          .Where(x => x.Id == body.IdProcess)
                                          .SingleOrDefaultAsync();
            if (process is null)
                throw new NotFoundException("process is not found");

            process.FinishedAt = DateTime.Now;
            _dbContext.Entity<TrGeneratedScheduleProcess>().Update(process);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2(null);
        }
    }
}
