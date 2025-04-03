using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnExtracurricular
{
    public interface IExtracurricularAttendance : IFnExtracurricular
    {
        [Post("/extracurricular-attendance/get-active-unsubmitted-attendance")]
        Task<ApiErrorResult<GetActiveUnsubmittedAttendanceResult>> GetActiveUnsubmittedAttendance([Body] GetActiveUnsubmittedAttendanceRequest body);

        [Get("/extracurricular-attendance/get-student-attendance")]
        Task<ApiErrorResult<GetStudentAttendanceResult>> GetStudentAttendance(GetStudentAttendanceRequest body);

        [Get("/extracurricular-attendance/get-attendance-status-by-school")]
        Task<ApiErrorResult<IEnumerable<GetAttendanceStatusBySchoolResult>>> GetAttendanceStatusBySchool(GetAttendanceStatusBySchoolRequest body);

        [Get("/extracurricular-attendance/get-student-attendance-detail")]
        Task<ApiErrorResult<IEnumerable<GetStudentAttendanceDetailResult>>> GetStudentAttendanceDetail(GetStudentAttendanceDetailRequest body);

        [Post("/extracurricular-attendance/update-extracurricular-attendance")]
        Task<ApiErrorResult> UpdateExtracurricularAttendance([Body] UpdateExtracurricularAttendanceRequest body);

        [Post("/extracurricular-attendance/add-session-extracurricular-attendance")]
        Task<ApiErrorResult> AddSessionExtracurricularAttendance([Body] AddSessionExtracurricularAttendanceRequest body);

        [Post("/extracurricular-attendance/export-excel-summary-extracurricular-attendance")]
        Task<HttpResponseMessage> ExportExcelSummaryExtracurricularAttendance([Body] ExportExcelSummaryExtracurricularAttendanceRequest body);

        [Delete("/extracurricular-attendance/delete-session-extracurricular-attendance")]
        Task<ApiErrorResult> DeleteSessionExtracurricularAttendance([Body] DeleteSessionExtracurricularAttendanceRequest body);

        [Post("/extracurricular-attendance/export-excel-unsubmiited-attendance")]
        Task<HttpResponseMessage> ExportExcelUnSubmittedScore(GetActiveUnsubmittedAttendanceRequest body);
    }
}
