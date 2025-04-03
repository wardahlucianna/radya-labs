using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerAllocation;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;

namespace BinusSchool.Student.FnStudent.LockerReservation.LockerAllocation
{
    public class GetLockerAllocationBuildingHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetLockerAllocationBuildingHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetLockerAllocationBuildingRequest>
                (nameof(GetLockerAllocationBuildingRequest.IdSchool));

            var getBuilding = _dbContext.Entity<MsFloor>()
                .Where(a => a.Building.IdSchool == param.IdSchool &&
                            a.HasLocker == true)
                .Distinct();

            var items = getBuilding.Select(a => new GetLockerAllocationBuildingResult
            {
                Id = a.IdBuilding,
                Description = a.Building.Description
            })
                .Distinct()
                .OrderBy(a => a.Description)
                .ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
