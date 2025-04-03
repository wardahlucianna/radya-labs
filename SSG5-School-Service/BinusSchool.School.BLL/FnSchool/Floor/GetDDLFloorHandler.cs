using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.Floor;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.Floor
{
    public class GetDDLFloorHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetDDLFloorHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDDLFloorRequest>(nameof(GetDDLFloorRequest.IdSchool), nameof(GetDDLFloorRequest.IdBuilding));

            var data = await _dbContext.Entity<MsFloor>()
                .Include(x => x.Building)
                .Where(x => x.Building.IdSchool == param.IdSchool)
                .Where(x => x.Building.Id == param.IdBuilding)
                .Select(x => new GetDDLFloorResponse
                {
                    Id = x.Id,
                    Description = x.Description,
                    ImageUrl = x.URL ?? "",
                    IsShowFloorLayout = x.IsShowFloorLayout ?? false
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(data as object);
        }
    }
}
