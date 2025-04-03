using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.School.FnSchool.VenueEquipment;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.VenueEquipment
{
    public class GetListVenueEquipmentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public GetListVenueEquipmentHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private static readonly string[] _columns = new[]
        {
            "Building",
            "Venue",
            "TotalEquipment",
        };

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListVenueEquipmentRequest>(nameof(GetListVenueEquipmentRequest.IdSchool));

            var query = _dbContext.Entity<MsVenueEquipment>()
                .Include(x => x.Venue).ThenInclude(x => x.Building)
                .Where(x => x.Venue.Building.IdSchool == param.IdSchool &&
                            (x.IdVenue == (param.Venue ?? x.IdVenue)) &&
                            (x.Venue.IdBuilding == (param.Building ?? x.Venue.IdBuilding)));

            if (!string.IsNullOrEmpty(param.SearchKey))
            {
                var searchKey = param.SearchKey.ToLower();
                query = query.Where(x => x.Venue.Description.ToLower().Contains(searchKey) ||
                                         x.Venue.Building.Description.ToLower().Contains(searchKey) ||
                                         _dbContext.Entity<MsVenueEquipment>()
                                             .Where(y => y.IdVenue == x.IdVenue)
                                             .Sum(y => y.EquipmentQty).ToString().Contains(searchKey));
            }
            
            var result = await query.ToListAsync(CancellationToken);

            var groupedResult = result
                .GroupBy(x => x.Venue)
                .Select(x => new GetListVenueEquipmentResult
                {
                    Building = x.Key.Building.Description,
                    Venue = x.Key.Description,
                    IdVenue = x.Key.Id,
                    TotalEquipment = x.Sum(y => y.EquipmentQty)
                })
                .ToList();

            query = param.OrderBy switch
            {
                "Building" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Venue.Building.Description)
                    : query.OrderByDescending(x => x.Venue.Building.Description),
                "Venue" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Venue.Description)
                    : query.OrderByDescending(x => x.Venue.Description),
                "TotalEquipment" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.EquipmentQty)
                    : query.OrderByDescending(x => x.EquipmentQty),
                _ => query.OrderBy(x => x.Venue.Building.Description)
            };

            var pagedResult = groupedResult
                .AsQueryable()
                .SetPagination(param)
                .ToList();

            var count = groupedResult.Count;

            return Request.CreateApiResult2(pagedResult as object, param.CreatePaginationProperty(count));
        }
    }
}
