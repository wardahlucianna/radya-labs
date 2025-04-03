using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministrationV2;
using Refit;
namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IAttendanceAdministrationV2: IFnAttendance
    {
        [Get("/attendance-administrationV2")]
        Task<ApiErrorResult<IEnumerable<GetAttendanceAdministrationV2Result>>> GetAttendancAdministrationV2(GetAttendanceAdministrationV2Request param);

        [Get("/attendance-administrationV2/{id}")]
        Task<ApiErrorResult<GetAttendanceAdministrationDetailV2Result>> GetAttendancAdministrationDetailV2(string id);

        [Post("/attendance-administrationV2")]
        Task<ApiErrorResult> AddAttendanceAdministrationV2([Body] PostAttendanceAdministrationV2Request body);

        [Get("/attendance-administrationV2-list-approval")]
        Task<ApiErrorResult<IEnumerable<GetListAttendanceAdministrationApprovalV2Result>>> GetAttendanceAdministrationApprovalV2(GetListAttendanceAdministrationApprovalV2Request param);

        [Put("/attendance-administrationV2-set-status-approval")]
        Task<ApiErrorResult> SetStatusApprovalAttendanceAdministrationV2([Body] SetStatusApprovalAttendanceAdministrationV2Request body);

        [Get("/attendance-administrationV2-get-subject")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetSubjectV2(GetAdministrationAttendanceSubjectRequest body);

        [Get("/attendance-administrationV2-cancel-attendance")]
        Task<ApiErrorResult<IEnumerable<GetCancelAttendanceResult>>> GetCancelAttendance(GetCancelAttendanceRequest param);

        [Post("/attendance-administrationV2-cancel-attendance")]
        Task<ApiErrorResult> CancelAttendance([Body] CancelAttendanceRequest query);
    }
}
