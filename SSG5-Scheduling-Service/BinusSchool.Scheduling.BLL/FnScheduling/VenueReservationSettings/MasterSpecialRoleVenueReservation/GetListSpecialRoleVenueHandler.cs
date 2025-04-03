using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservationSettings.MasterSpecialRoleVenueReservation;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;


namespace BinusSchool.Scheduling.FnSchedule.VenueReservationSettings.MasterSpecialRoleVenueReservation
{
    public class GetListSpecialRoleVenueHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetListSpecialRoleVenueHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListSpecialRoleVenueRequest>(nameof(GetListSpecialRoleVenueRequest.IdSchool));

            var column = new[] { "rolename", "specialdurationbooking" };

            var predicate = PredicateBuilder.Create<MsSpecialRoleVenue>(x => x.Role.IdSchool == param.IdSchool);
            if (!string.IsNullOrWhiteSpace(param.Search))
            {
                predicate = predicate.And(x 
                    => EF.Functions.Like(x.Role.Description, param.SearchPattern())
                    || EF.Functions.Like(x.SpecialDurationBookingTotalDay.ToString(), param.SearchPattern()));
            }

            var query = _dbContext.Entity<MsSpecialRoleVenue>()
                .Where(predicate)
                .OrderByDynamic(param);

            query = param.OrderBy.ToLower() switch
            {
                "rolename" => param.OrderType == OrderType.Asc ? query.OrderBy(x => x.Role.Description) : query.OrderByDescending(x => x.Role.Description),
                "specialdurationbooking" => param.OrderType == OrderType.Asc ? query.OrderBy(x => x.SpecialDurationBookingTotalDay) : query.OrderByDescending(x => x.SpecialDurationBookingTotalDay),
                _ => query.OrderByDynamic(param)
            };

            var data = await query
                .SetPagination(param)
                .Select(x => new GetListSpecialRoleVenueResult
                {
                    IdSpecialRoleVenue = x.Id,
                    RoleDescription = x.Role.Description,
                    SpecialDurationBookingTotalDay = x.SpecialDurationBookingTotalDay
                }).ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(data.Count) ? data.Count : await query.Select(x => x.Id).CountAsync();

            return Request.CreateApiResult2(data as object, param.CreatePaginationProperty(count).AddColumnProperty(column));
        }
    }
}
