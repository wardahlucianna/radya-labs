using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.ApprovalAttendanceAdministration;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using Refit;
namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IAttendanceAdministration: IFnAttendance
    {
        [Get("/attendance-administration")]
        Task<ApiErrorResult<IEnumerable<GetAttendanceAdministrationResult>>> GetAttendancAdministration(GetAttendanceAdministrationRequest param);

        [Get("/attendance-administration/{id}")]
        Task<ApiErrorResult<GetAttendanceAdministrationDetailResult>> GetAttendancAdministrationDetail(string id);

        [Post("/attendance-administration")]
        Task<ApiErrorResult> AddAttendanceAdministration([Body] PostAttendanceAdministrationRequest body);

        [Post("/attendance-administration-summary")]
        Task<ApiErrorResult<GetAttendanceAdministrationSummaryResult>> GetSummary([Body] GetAttendanceAdministrationSummaryRequest body);

        [Get("/attendance-administration-get-students")]
        Task<ApiErrorResult<IEnumerable<GetAdministrationAttendanceStudentResult>>> GetStudentAttendancAdministration(GetAdministrationAttendanceStudentRequest param);

        [Get("/attendance-administration-get-homeroom")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetHomeroomAttendancAdministration(GetAdministrationAttendanceHomeroomRequest param);

        [Get("/attendance-administration-get-subject")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetSubjectAttendancAdministration(GetAdministrationAttendanceSubjectRequest param);

        [Get("/attendance-administration-list-approval")]
        Task<ApiErrorResult<IEnumerable<GetListAttendanceAdministrationApprovalResult>>> GetAttendanceAdministrationApproval(GetListAttendanceAdministrationApprovalRequest param);

        [Put("/attendance-administration-set-status-approval")]
        Task<ApiErrorResult> SetStatusApprovalAttendanceAdministration([Body] SetStatusApprovalAttendanceAdministrationRequest body);
    }
}
