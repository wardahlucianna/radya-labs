using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.Category;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.Category
{
    public class GetAttendanceCategoryHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetAttendanceCategoryHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceCategoryRequest>(nameof(GetAttendanceCategoryRequest.IdAcadyear),
                nameof(GetAttendanceCategoryRequest.IdLevel));
            var predicate = PredicateBuilder.Create<MsAttendanceMappingAttendance>(x => x.MappingAttendance.IdLevel == param.IdLevel);
            if (param.AttendanceCategory.HasValue)
                predicate = predicate.And(x => x.Attendance.AttendanceCategory == param.AttendanceCategory.Value);

            var query = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                .Include(x => x.Attendance)
                .Where(predicate)
                .ToListAsync(CancellationToken);
            var result = new GetAttendanceCategoryResult();
            result.AttendanceCategory = param.AttendanceCategory.Value;
            result.PresentAttendances = query
            .Where(x => x.Attendance.AttendanceCategory == AttendanceCategory.Present)
            .Select(x => new AttendanceCategoryAttendance
            {
                Id = x.Attendance.Id,
                Description = x.Attendance.Description,
                Code = x.Attendance.Code,
                NeedAttachment = x.Attendance.IsNeedFileAttachment
            }).ToList();

            result.ExcusedAttendance = query
            .Where(x => x.Attendance.AttendanceCategory == AttendanceCategory.Absent)
            .Where(x => x.Attendance.AbsenceCategory.Value == AbsenceCategory.Excused)
            .GroupBy(x => x.Attendance.ExcusedAbsenceCategory.Value)
            .ToDictionary(x => x.Key, x => x.Select(x => new AttendanceCategoryAttendance
            {
                Id = x.Attendance.Id,
                Code = x.Attendance.Code,
                Description = x.Attendance.Description,
                NeedAttachment = x.Attendance.IsNeedFileAttachment
            }));

            result.UnexcusedAttendance = query
            .Where(x => x.Attendance.AttendanceCategory == AttendanceCategory.Absent)
                        .Where(x => x.Attendance.AbsenceCategory.Value == AbsenceCategory.Unexcused)
            .GroupBy(x => x.Attendance.AbsenceCategory.Value)
            .ToDictionary(x => x.Key, x => x.Select(x => new AttendanceCategoryAttendance
            {
                Id = x.Attendance.Id,
                Code = x.Attendance.Code,
                Description = x.Attendance.Description,
                NeedAttachment = x.Attendance.IsNeedFileAttachment
            }));


            return Request.CreateApiResult2(result as object);
        }
    }
}
