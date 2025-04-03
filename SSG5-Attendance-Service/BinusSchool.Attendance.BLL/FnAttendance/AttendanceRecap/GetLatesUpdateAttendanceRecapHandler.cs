using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceRecap
{
    public class GetLatesUpdateAttendanceRecapHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        public GetLatesUpdateAttendanceRecapHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var LastUpdate = await _dbContext.Entity<TrAttendanceSummaryLog>()
               .Where(x => x.IsDone)
               .OrderByDescending(e => e.StartDate)
               .Select(e => e.StartDate)
               .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(LastUpdate as object);
        }
    }
}
