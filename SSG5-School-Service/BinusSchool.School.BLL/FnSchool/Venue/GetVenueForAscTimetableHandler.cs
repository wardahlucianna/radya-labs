using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.Venue;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.Venue
{
    public class GetVenueForAscTimetableHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetVenueForAscTimetableHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetVenueForAscTimetableRequest>(nameof(GetVenueForAscTimetableRequest.VenueCode));
            var getData = await _dbContext.Entity<MsVenue>()
                                            .Include(p => p.Building)
                                            .Where(p => param.VenueCode.Any(x => x == p.Code) && p.Building.IdSchool == param.IdSchool)
                                            .Select(p => new GetVenueForAscTimetableResult
                                            {
                                                IdVenue=p.Id,
                                                Code=p.Code,
                                                Description=p.Description,
                                                BuildingCode=p.Building.Code,
                                                BuildingDescription=p.Building.Description,
                                            }).ToListAsync();

            return Request.CreateApiResult2(getData as object);
        }
    }
}
