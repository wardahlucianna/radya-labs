using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceBlockingSetting;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceBlockingSetting
{
    public class GetAttendanceBlockingSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetAttendanceBlockingSettingHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceBlockingSettingDetailRequest>(nameof(GetAttendanceBlockingSettingDetailRequest.IdAcadyear));

            var query = await _dbContext.Entity<MsBlockingTypeAtdSetting>()
                   .Include(x => x.BlockingType)
                   .Include(x => x.AcademicYear)
                   .Include(x => x.Level)
                   .Include(x => x.AtdMappingAtd)
                   .Where(x => x.IdAcademicYear == param.IdAcadyear.ToString() & x.IdBlockingType == param.IdBlockingType)
                   .Select(x => new GetAttendanceBlockingSettingDetailResult
                   {
                       Id = x.Level.Id,
                       Code = x.Level.Code,
                       Description = x.Level.Description,
                       AtdMappingAtd = new MapAttendanceDetail
                       {
                           Id = x.AtdMappingAtd.IdAttendance,
                           IdAtdMappingAtd = x.AtdMappingAtd.Id,
                           Code = x.AtdMappingAtd.Attendance.Code,
                           Description = x.AtdMappingAtd.Attendance.Description,
                       },
                   })
                   .OrderBy(x => x.Description)
                   .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(query as object);
        }
    }
}
