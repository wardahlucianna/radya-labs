using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.VenueEquipment;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.VenueEquipment
{
    public class GetVenueEquipmentDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public GetVenueEquipmentDetailHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetVenueEquipmentDetailRequest>(nameof(GetVenueEquipmentDetailRequest.IdVenue));

            var query = await _dbContext.Entity<MsVenueEquipment>()
                .Include(x => x.Venue)
                    .ThenInclude(x => x.Building)
                .Include(x => x.Equipment)
                    .ThenInclude(x => x.EquipmentType)
                        .ThenInclude(x => x.ReservationOwner)
                .Where(x => x.IdVenue == param.IdVenue)
                .ToListAsync(CancellationToken);

            var data = query
                .GroupBy(x => new
                {
                    x.IdVenue,
                    x.Venue.Description,
                    x.Venue.IdBuilding,
                    BuildingDescription = x.Venue.Building.Description
                })
                .Select(y => new GetVenueEquipmentDetailResult
                {
                    IdVenue = y.Key.IdVenue,
                    VenueName = y.Key.Description,
                    IdBuilding = y.Key.IdBuilding,
                    BuildingName = y.Key.BuildingDescription,
                    VenueEquipmentDetails = y.Select(y => new VenueEquipmentDetail
                    {
                        IdVenueEquipment = y.Id,
                        IdEquipment = y.IdEquipment,
                        EquipmentName = y.Equipment.EquipmentName,
                        EquipmentType = y.Equipment.EquipmentType.EquipmentTypeName,
                        Owner = y.Equipment.EquipmentType.ReservationOwner.OwnerName,
                        EquipmentQty = y.EquipmentQty
                    }).ToList()
                }).FirstOrDefault();

            return Request.CreateApiResult2(data as object);
        }
    }
}
