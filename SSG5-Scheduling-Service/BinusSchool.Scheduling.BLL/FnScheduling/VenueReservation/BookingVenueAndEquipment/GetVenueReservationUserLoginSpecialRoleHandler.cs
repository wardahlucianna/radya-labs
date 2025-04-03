using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment
{
    public class GetVenueReservationUserLoginSpecialRoleHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetVenueReservationUserLoginSpecialRoleHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetVenueReservationUserLoginSpecialRoleRequest>
                (nameof(GetVenueReservationUserLoginSpecialRoleRequest.IdUser));

            var response = await GetVenueReservationUserLoginSpecialRole(new GetVenueReservationUserLoginSpecialRoleRequest
            {
                IdUser = request.IdUser
            });

            return Request.CreateApiResult2(response as object);
        }

        public async Task<GetVenueReservationUserLoginSpecialRoleResponse> GetVenueReservationUserLoginSpecialRole(GetVenueReservationUserLoginSpecialRoleRequest request)
        {
            var getUserSpecialRole = await _dbContext.Entity<MsSpecialRoleVenue>()
                .ToListAsync();

            var getUserRole = await _dbContext.Entity<MsUserRole>()
                .Where(a => a.IdUser == request.IdUser)
                .ToListAsync();

            var joinData = from userSpecialRole in getUserSpecialRole
                           join userRole in getUserRole on userSpecialRole.IdRole equals userRole.IdRole
                           select new
                           {
                               UserSpecialRole = userSpecialRole,
                               UserRole = userRole
                           };

            var response = joinData
                .Select(a => new GetVenueReservationUserLoginSpecialRoleResponse
                {
                    SpecialDurationBookingTotalDay = a.UserSpecialRole.SpecialDurationBookingTotalDay,
                    CanOverrideAnotherReservation = a.UserSpecialRole?.CanOverrideAnotherReservation ?? false,
                    AllSuperAccess = a.UserSpecialRole?.AllSuperAccess ?? false
                })
                .FirstOrDefault();

            return response;
        }
    }
}
