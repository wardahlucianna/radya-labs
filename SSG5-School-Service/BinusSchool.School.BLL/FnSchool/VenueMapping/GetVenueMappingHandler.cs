using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.VenueMapping;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
namespace BinusSchool.School.FnSchool.VenueMapping
{
    public class GetVenueMappingHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetVenueMappingHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetVenueMappingRequest>(nameof(GetVenueMappingRequest.IdAcademicYear));

            var getVenue = await _dbContext.Entity<MsVenue>()
                .Include(x => x.Building)
                .Where(x => x.Building.IdSchool == param.IdSchool)
                .Where(x => string.IsNullOrEmpty(param.IdBuilding) ? true : x.IdBuilding == param.IdBuilding)
                .ToListAsync(CancellationToken);

            var getVenueMapping = await _dbContext.Entity<MsVenueMapping>()
                .Include(x => x.Floor)
                .Include(x => x.VenueType)
                .Include(x => x.ReservationOwner)
                .Include(x => x.VenueMappingApprovals).ThenInclude(x => x.Staff)
                .Where(x => x.IdAcademicYear == param.IdAcademicYear)
                .Where(x => string.IsNullOrEmpty(param.IdBuilding) ? true : x.Venue.IdBuilding == param.IdBuilding)
                .ToListAsync(CancellationToken);

            var res = getVenue.GroupJoin(getVenueMapping,
                (venue) => venue.Id,
                (mapping) => mapping.IdVenue,
                (venue, mapping) => new { venue, mapping = mapping.DefaultIfEmpty() })
                .SelectMany(x => x.mapping.DefaultIfEmpty(), 
                (x, mapping) => new GetVenueMappingResult
                {
                    Description = mapping?.Description,
                    Venue = new ItemValueVm
                    {
                        Id = x.venue.Id,
                        Description = x.venue.Description
                    },
                    Building = new ItemValueVm
                    {
                        Id = x.venue.IdBuilding,
                        Description = x.venue.Building.Description
                    },
                    Floor = new NullableItemValueVm
                    {
                        Id = mapping?.IdFloor,
                        Description = mapping?.Floor.Description
                    },
                    VenueType = new NullableItemValueVm
                    {
                        Id = mapping?.IdVenueType,
                        Description = mapping?.VenueType.Description
                    },
                    Owner = new NullableNameValueVm
                    {
                        Id = mapping?.IdReservationOwner,
                        Name = mapping?.ReservationOwner.OwnerName
                    },
                    NeedApproval = mapping?.VenueMappingApprovals?
                        .Select(y => new NullableNameValueVm
                        {
                            Id = y.IdBinusian,
                            Name = NameUtil.GenerateFullName(y.Staff.FirstName, y.Staff.LastName)
                        }).ToList() ?? null,
                    VenueStatus = mapping?.IsVenueActive ?? true,
                    LastSaved = (mapping == null ? null : (mapping.DateUp ?? mapping.DateIn)),
                    LastSavedBy = (mapping == null ? null : (mapping.UserUp ?? mapping.UserIn))
                }).ToList();

            res.ForEach(x => x.NeedApproval = x.NeedApproval == null ? null : (x.NeedApproval.Count() == 0 ? null : x.NeedApproval));

            return Request.CreateApiResult2(res as object);
        }
    }
}
