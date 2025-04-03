using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using System.Threading.Tasks;
using System.Linq;
using BinusSchool.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.Data.Model.School.FnSchool.SurveySummary;

namespace BinusSchool.Attendance.FnAttendance.SurveySummary
{

    public class GetSurveySummaryLogHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public GetSurveySummaryLogHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSurveySummaryLogRequest>();

            var surveySummaryLog = await _dbContext.Entity<TrSurveySummaryLog>()
                                      .Where(e => e.UserIn == param.IdUser && e.IsProcess)
                                      .ToListAsync(CancellationToken);

            GetSurveySummaryLogResult surveySummary = new GetSurveySummaryLogResult
            {
                IsStillProses = surveySummaryLog.Any()
            };

            return Request.CreateApiResult2(surveySummary as object);
        }
    }
}
