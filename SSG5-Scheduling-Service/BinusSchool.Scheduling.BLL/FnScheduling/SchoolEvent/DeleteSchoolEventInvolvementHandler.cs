using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Scheduling.FnSchedule.SchoolEvent.Validator;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class DeleteSchoolEventInvolvementHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public DeleteSchoolEventInvolvementHandler(ISchedulingDbContext schedulingDbContext)
        {
            _dbContext = schedulingDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<DeleteSchoolEventInvolvementRequest, DeleteSchooEvenetInvolvementValidator>();

            var data = await _dbContext.Entity<TrEvent>().FindAsync(body.IdEvent);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Event"], "Id", body.IdEvent));

            var dataDeclined = await _dbContext.Entity<TrEvent>()
                .Where(x => x.Id == body.IdEvent && x.StatusEvent == "Declined")
                .FirstOrDefaultAsync(CancellationToken);
            if (dataDeclined is null)
                throw new BadRequestException($"Event involvement can not delete, because certificate template on review/approved");

            data.IsActive = false;

            _dbContext.Entity<TrEvent>().Update(data);

            var GetEventDetail = await _dbContext.Entity<TrEventDetail>()
               .Where(x => body.IdEvent.Contains(x.IdEvent))
               .ToListAsync(CancellationToken);

            foreach (var itemDetail in GetEventDetail)
            {
                itemDetail.IsActive = false;
                _dbContext.Entity<TrEventDetail>().Update(itemDetail);
            }

            var GetEventActivity = await _dbContext.Entity<TrEventActivity>()
                .Include(e => e.EventActivityAwards)
               .Where(x => body.IdEvent.Contains(x.IdEvent))
               .ToListAsync(CancellationToken);

            foreach (var itemActivity in GetEventActivity)
            {
                itemActivity.IsActive = false;
                itemActivity.EventActivityAwards.ToList().ForEach(e => e.IsActive = false);
                itemActivity.IsActive = false;
                _dbContext.Entity<TrEventActivity>().Update(itemActivity);

                if (itemActivity.EventActivityAwards.Any())
                {
                    itemActivity.EventActivityAwards.ToList().ForEach(e => e.IsActive = false);
                    _dbContext.Entity<TrEventActivityAward>().UpdateRange(itemActivity.EventActivityAwards);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
