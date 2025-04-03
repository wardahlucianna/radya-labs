using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using System.Threading.Tasks;
using BinusSchool.Scheduling.FnSchedule.SchoolEvent.Validator;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Abstractions;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class UpdateStatusSyncEventScheduleHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _datetime;

        public UpdateStatusSyncEventScheduleHandler(ISchedulingDbContext schedulingDbContext, IMachineDateTime datetime)
        {
            _dbContext = schedulingDbContext;
            _datetime = datetime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateStatusSyncEventScheduleRequest, UpdateStatusSyncEventScheduleValidator>();

            var getEventSchedule = await _dbContext.Entity<TrEventSchedule>()
                .Where(e=>e.Id==body.IdEventSchedule)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(CancellationToken);

            if (getEventSchedule != null)
            {
                getEventSchedule.IsSyncAttendance = true;
                getEventSchedule.DateSyncAttendance = _datetime.ServerTime;
                _dbContext.Entity<TrEventSchedule>().Update(getEventSchedule);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
