using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.ApprovalAttendanceAdministration;
using Refit;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IApprovalAttendanceAdministration : IFnAttendance
    {
        [Get("/approval-attendance-administration/{id}")]
        Task<ApiErrorResult<ApprovalAttendanceAdministrationResponse>> GetAttendancAdministrationDetail(string id);

        [Post("/approval-attendance-administration")]
        Task<ApiErrorResult> AddAttendanceAdministration([Body] ApprovalAttendanceAdministrationRequest body);
    }
}
