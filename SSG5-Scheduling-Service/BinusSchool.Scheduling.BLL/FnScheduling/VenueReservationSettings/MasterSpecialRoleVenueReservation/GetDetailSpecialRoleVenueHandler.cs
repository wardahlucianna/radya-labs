using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservationSettings.MasterSpecialRoleVenueReservation;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservationSettings.MasterSpecialRoleVenueReservation
{
    public class GetDetailSpecialRoleVenueHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetDetailSpecialRoleVenueHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailSpecialRoleVenueRequest>(nameof(GetDetailSpecialRoleVenueRequest.IdSpecialRoleVenue));

            var data = await _dbContext.Entity<MsSpecialRoleVenue>()
                .Include(x => x.Role)
                .Where(x => x.Id == param.IdSpecialRoleVenue)
                .Select(x => new GetDetailSpecialRoleVenueResult
                {
                    IdSpecialRoleVenue = x.Id,
                    Role = new ItemValueVm
                    {
                        Id = x.Role.Id,
                        Description = x.Role.Description
                    },
                    SpecialDurationBookingTotalDay = x.SpecialDurationBookingTotalDay,
                    CanOverrideAnotherReservation = x.CanOverrideAnotherReservation,
                    AllSuperAccess = x.AllSuperAccess
                })
                .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(data as object);
        }
    }
}
