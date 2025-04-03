using System;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.Formula;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.Formula
{
    public class GetFormulaDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetFormulaDetailHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            if (!KeyValues.TryGetValue("idLevel", out var idLevel))
                throw new ArgumentNullException(nameof(idLevel));

            var data = await _dbContext.Entity<MsFormula>()
                                       .FirstOrDefaultAsync(x => x.IdLevel == (string)idLevel);

            var result = data != null ? new FormulaResult
            {
                AttendanceRate = data.AttendanceRate,
                PresenceInClass = data.PresenceInClass
            } : new FormulaResult();
            return Request.CreateApiResult2(result as object);
        }
    }
}
