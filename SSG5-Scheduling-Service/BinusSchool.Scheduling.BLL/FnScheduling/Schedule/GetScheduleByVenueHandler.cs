using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;


namespace BinusSchool.Scheduling.FnSchedule.Schedule
{
    public class GetScheduleByVenueHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetScheduleByVenueHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetScheduleByVenueRequest>(nameof(GetScheduleByVenueRequest.IdVenue));
            var anyData = false;
            var scheduleData = await _dbContext.Entity<MsSchedule>()
                               .Where(x => x.IdVenue == param.IdVenue).OrderByDescending(x => x.DateIn).FirstOrDefaultAsync(CancellationToken);

            if (scheduleData != null)
                anyData = true;
            return Request.CreateApiResult2(anyData as object);
        }
    }
}
