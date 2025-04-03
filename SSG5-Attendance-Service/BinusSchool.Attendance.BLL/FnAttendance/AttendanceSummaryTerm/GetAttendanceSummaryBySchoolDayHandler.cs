using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryBySchoolDayHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetAttendanceSummaryBySchoolDayHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryBySchoolDayRequest>();

            List<string> listIdLesson = new List<string>();

            var listStatusStudent = await _dbContext.Entity<TrAttendanceEntryV2>()
                .Include(x => x.HomeroomStudent)
                    .ThenInclude(x => x.Student)
                                .Where(e => e.IdScheduleLesson == param.IdScheduleLesson)
                                .Select(e => new GetAttendanceSummaryBySchoolDayResult
                                {
                                    IdStudent = e.HomeroomStudent.Student.Id,
                                    Name = string.Concat(e.HomeroomStudent.Student.FirstName, e.HomeroomStudent.Student.MiddleName, e.HomeroomStudent.Student.LastName),
                                    Status = param.Status
                                })
                                .ToListAsync(CancellationToken);            

            return Request.CreateApiResult2(listStatusStudent as object);
        }
    }
}
