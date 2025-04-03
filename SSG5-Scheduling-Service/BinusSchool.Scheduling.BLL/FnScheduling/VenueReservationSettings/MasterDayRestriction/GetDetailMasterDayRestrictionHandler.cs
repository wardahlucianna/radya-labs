using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservationSettings.MasterDayRestriction;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservationSettings.MasterDayRestriction
{
    public class GetDetailMasterDayRestrictionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetDetailMasterDayRestrictionHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailMasterDayRestrictionRequest>(nameof(GetDetailMasterDayRestrictionRequest.IdGroupRestriction));

            var getRestrictionBookingVenue = await _dbContext.Entity<MsRestrictionBookingVenue>()
                .Include(x => x.Building)
                .ToListAsync(CancellationToken);

            var filteredRestrictionBookingVenue = getRestrictionBookingVenue.Where(x => x.IdGroupRestriction == param.IdGroupRestriction);

            var listBuilding = filteredRestrictionBookingVenue.Select(x => new CodeWithIdVm
            {
                Id = x.Building.Id,
                Code = x.Building.Code,
                Description = x.Building.Description
            }).ToList();

            var listVenue = filteredRestrictionBookingVenue.Where(x => x.IdVenue != null).Select(x => x.IdVenue).Distinct().ToList(); 

            var getVenue = new List<ItemValueVm>();
            if(listVenue.Count() != 0)
            {
                getVenue = await _dbContext.Entity<MsVenue>()
                    .Where(x => listVenue.Contains(x.Id))
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Id,
                        Description = x.Description
                    })
                    .ToListAsync(CancellationToken);
            }

            var res = new GetDetailMasterDayRestrictionResult
            {
                IdGroupRestriction = filteredRestrictionBookingVenue.FirstOrDefault().IdGroupRestriction,
                StartRestrictionDate = filteredRestrictionBookingVenue.FirstOrDefault().StartRestrictionDate,
                EndRestrictionDate = filteredRestrictionBookingVenue.FirstOrDefault().EndRestrictionDate,
                ListBuilding = listVenue.Count() == 0 ? listBuilding : null,
                ListVenue = getVenue
            };

            return Request.CreateApiResult2(res as object);


        }
    }
}
