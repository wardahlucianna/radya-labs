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
    public class GetListFloorHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetListFloorHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListFloorRequest>(nameof(GetListFloorRequest.IdSchool));

            var data = await _dbContext.Entity<MsFloor>()
                .Include(x => x.Building)
                .Where(x => x.Building.IdSchool == param.IdSchool)
                .Select(x => new GetListFloorResult
                {
                    IdFloor = x.Id,
                    Building = new ItemValueVm
                    {
                        Id = x.Building.Id,
                        Description = x.Building.Description
                    },
                    FloorName = x.FloorName,
                    Description = x.Description,
                    CanDelete = !(x.VenueMappings.Any() || x.Lockers.Any())
                }).ToListAsync(CancellationToken);

            return Request.CreateApiResult2(data as object);
        }
    }
}
