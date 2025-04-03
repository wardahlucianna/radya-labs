using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;

namespace BinusSchool.Attendance.FnAttendance.Abstractions
{
    public interface IAttendanceSummaryRedisService : IAttendanceSummaryService
    {
        Task<List<string>> GetLessonByUser(GetHomeroomByIdUserRequest param, CancellationToken cancellationToken);
    }
}
