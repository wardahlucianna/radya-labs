using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.MasterVenueReservationRule;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.MasterVenueReservationRule
{
    public class GetMasterVenueReservationRuleHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetMasterVenueReservationRuleHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMasterVenueReservationRuleRequest>(nameof(GetMasterVenueReservationRuleRequest.IdSchool));

            var data = await _dbContext.Entity<MsVenueReservationRule>()
                .Where(x => x.IdSchool == param.IdSchool)
                .Select(x => new GetMasterVenueReservationRuleResult
                {
                    IdVenueReservationRule = x.Id,
                    MaxDayBookingVenue = x.MaxDayBookingVenue,
                    MaxTimeBookingVenue = x.MaxTimeBookingVenue,
                    MaxDayDurationBookingVenue = x.MaxDayDurationBookingVenue,
                    VenueNotes = x.VenueNotes,
                    StartTimeOperational = x.StartTimeOperational,
                    EndTimeOperational = x.EndTimeOperational,
                    CanBookingAnotherUser = x.CanBookingAnotherUser,
                    LastUserUpdated = x.UserUp == null ? x.UserIn : x.UserUp,
                    LastDateUpdated = x.DateUp == null ? x.DateIn : x.DateUp
                })
                .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(data as object);
        }
    }
}
