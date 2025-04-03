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
    public class GetDetailFloorHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetDetailFloorHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailFloorRequest>(nameof(GetDetailFloorRequest.IdFloor));

            var data = await _dbContext.Entity<MsFloor>()
                .Where(x => x.Id == param.IdFloor)
                .Select(x => new GetDetailFloorResult
                {
                    IdFloor = x.Id,
                    FloorName = x.FloorName,
                    Building = new ItemValueVm
                    {
                        Id = x.Building.Id,
                        Description = x.Building.Description
                    },
                    HasLocker = x.HasLocker,
                    LockerTowerCodeName = x.LockerTowerCodeName,
                    Description = x.Description,
                    URL = x.URL,
                    FileName = x.FileName,
                    FileType = x.FileType,
                    FileSize = x.FileSize,
                    IsShowFloorLayout = x.IsShowFloorLayout
                }).FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(data as object);
        }
    }
}
