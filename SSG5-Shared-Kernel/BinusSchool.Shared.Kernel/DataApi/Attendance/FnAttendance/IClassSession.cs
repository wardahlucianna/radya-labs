using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.ClassSession;
using Refit;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IClassSession : IFnAttendance
    {
        [Get("/class-and-session")]
        Task<ApiErrorResult<IEnumerable<GetClassSessionResult>>> GetClassAndSessions(GetClassSessionRequest param);

        [Get("/class-and-session-V2")]
        Task<ApiErrorResult<IEnumerable<GetClassSessionResult>>> GetClassAndSessionsV2(GetClassSessionRequest param);
    }
}
