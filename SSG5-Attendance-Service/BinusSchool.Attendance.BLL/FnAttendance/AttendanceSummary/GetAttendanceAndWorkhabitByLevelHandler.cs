using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary
{
    public class GetAttendanceAndWorkhabitByLevelHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetAttendanceAndWorkhabitByLevelHandler(IAttendanceDbContext dbContext
            )
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceAndWorkhabitByLevelRequest>(
              nameof(GetAttendanceAndWorkhabitByLevelRequest.IdLevel));

            var dataAttendanceByLevel = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                .Include(x => x.MappingAttendance)
                .Include(x => x.Attendance)
                .Where(x => x.MappingAttendance.IdLevel == param.IdLevel)
                .Select(x => new CodeWithIdVm
                {
                    Id = x.Id,
                    Code = x.Attendance.Code,
                    Description = x.Attendance.Description
                }).ToListAsync();

            var dataWorkhabitByLevel = await _dbContext.Entity<MsMappingAttendanceWorkhabit>()
                .Include(x => x.MappingAttendance)
                .Include(x => x.Workhabit)
                .Where(x => x.MappingAttendance.IdLevel == param.IdLevel)
                 .Select(x => new CodeWithIdVm
                 {
                     Id = x.Id,
                     Code = x.Workhabit.Code,
                     Description = x.Workhabit.Description
                 }).ToListAsync();

            var res = new GetAttendanceAndWorkhabitByLevelResult
            {
                Workhabits = dataWorkhabitByLevel,
                Attendances = dataAttendanceByLevel
            };
            return Request.CreateApiResult2(res as object);
        }
    }
}
