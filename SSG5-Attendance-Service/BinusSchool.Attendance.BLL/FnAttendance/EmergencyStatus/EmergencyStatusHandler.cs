using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;


namespace BinusSchool.Attendance.FnAttendance.EmergencyStatus
{
    public class EmergencyStatusHandler : FunctionsHttpCrudHandler
    {

        private readonly IAttendanceDbContext _dbContext;
        public EmergencyStatusHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var query = _dbContext.Entity<LtEmergencyStatus>();
            IReadOnlyList<IItemValueVm> items;
            items = await query
                    .Select(x => new CodeWithIdVm
                    {
                        Id = x.Id,
                        Description = x.EmergencyStatusName,
                        Code = x.ColorCode
                    })
                    .OrderBy(x => x.Id)
                    .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(items);
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
