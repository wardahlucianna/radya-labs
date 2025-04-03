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
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.LockerReservation.LockerAllocation
{
    public class GetLockerAllocationFloorHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetLockerAllocationFloorHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetLockerAllocationFloorRequest>
                (nameof(GetLockerAllocationFloorRequest.IdSchool),
                nameof(GetLockerAllocationFloorRequest.IdBuilding));

            var getFloor = _dbContext.Entity<MsFloor>()
                .Include(a => a.Building)
                .Where(a => a.Building.IdSchool == param.IdSchool
                    && a.IdBuilding == param.IdBuilding
                    && a.HasLocker == true)
                .Distinct();

            var items = getFloor.Select(a => new GetLockerAllocationFloorResult
            {
                Id = a.Id,
                Description = a.FloorName
            })
                .Distinct()
                .OrderBy(a => a.Description)
                .ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
