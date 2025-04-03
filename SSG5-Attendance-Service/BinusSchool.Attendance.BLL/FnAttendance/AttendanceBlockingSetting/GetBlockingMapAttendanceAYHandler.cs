using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceBlockingSetting;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceBlockingSetting
{
    public class GetBlockingMapAttendanceAYHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetBlockingMapAttendanceAYHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetBlockingMapAttendanceAYRequest>(nameof(GetBlockingMapAttendanceAYRequest.IdAcadyear));

            var query = await _dbContext.Entity<MsLevel>()
                    .Include(x=>x.MappingAttendances).ThenInclude(x=>x.AttendanceMappingAttendances)
                    .Where(x => x.IdAcademicYear == param.IdAcadyear.ToString())
                    .Select(x => new GetBlockingMapAttendanceAYResult
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description,
                        MapAttendanceItems = x.MappingAttendances.SelectMany(y=> y.AttendanceMappingAttendances.Where(z=> z.Attendance.AttendanceCategory == AttendanceCategory.Absent)).Select(y => new MapAttendanceItem
                        {
                            Id = y.IdAttendance,
                            IdAtdMappingAtd = y.Id,
                            Code = y.Attendance.Code,
                            Description = y.Attendance.Description,
                        }).ToList(),
                    })
                    .OrderBy(x=>x.Description)
                    .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(query as object);
        }      
    }
}
