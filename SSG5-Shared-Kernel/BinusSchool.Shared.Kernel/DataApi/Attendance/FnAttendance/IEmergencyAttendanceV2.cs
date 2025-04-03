using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.EventAttendanceEntry;
using Refit;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2;
using BinusSchool.Data.Models.Binusian.BinusSchool.AttendanceLog;
using BinusSchool.Data.Model.Document.FnDocument.BLPGroup;
using System.Net.Http;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IEmergencyAttendanceV2 : IAttendance
    {
        [Get("/emergency-attendance-v2/student-emergency-attendance")]
        Task<ApiErrorResult<GetStudentEmergencyAttendanceResult>> GetStudentEmergencyAttendance(GetStudentEmergencyAttendanceRequest param);

        [Get("/emergency-attendance-v2/get-emergency-attendance-summary")]
        Task<ApiErrorResult<GetEmergencyAttendanceSummaryResult>> GetEmergencyAttendanceSummary(GetEmergencyAttendanceSummaryRequest param);

        [Post("/emergency-attendance-v2/save-student-emergency-attendace")]
        Task<ApiErrorResult<SaveStudentEmergencyAttendanceResult>> SaveStudentEmergencyAttendance([Body] SaveStudentEmergencyAttendanceRequest body);

        [Get("/emergency-attendance-v2/get-emergency-attendance-privilege")]
        Task<ApiErrorResult<GetEmergencyAttendancePrivilegeResult>> GetEmergencyAttendancePrivilege(GetEmergencyAttendancePrivilegeRequest param);

        [Post("/emergency-attendance-v2/update-emergency-attendace-report")]
        Task<ApiErrorResult<UpdateEmergencyAttendanceReportResult>> UpdateEmergencyAttendanceReport([Body] UpdateEmergencyAttendanceReportRequest body);

        [Get("/emergency-attendance-v2/get-emergency-report-list")]
        Task<ApiErrorResult<IEnumerable<GetEmergencyReportListResult>>> GetEmergencyReportList(GetEmergencyReportListRequest param);

        [Get("/emergency-attendance-v2/get-emergency-report-detail")]
        Task<ApiErrorResult<IEnumerable<GetEmergencyReportDetailResult>>> GetEmergencyReportDetail(GetEmergencyReportDetailRequest param);

        [Post("/emergency-attendance-v2/emergency-report-detail-excel")]
        Task<HttpResponseMessage> ExportExcelEmergencyReportDetail([Body] ExportExcelEmergencyReportDetailRequest body);

    }
}
