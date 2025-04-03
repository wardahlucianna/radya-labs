using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using Refit;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IAttendanceSummaryTerm : IFnAttendance
    {
        #region Attendance Summary
        [Get("/attendance-summary-term")]
        Task<ApiErrorResult<IEnumerable<GetAttendanceSummaryResult>>> GetAttendanceSummary(GetAttendanceSummaryRequest query);

        [Get("/attendance-summary-term/widge")]
        Task<ApiErrorResult<GetAttendanceSummaryWidgeResult>> GetAttendanceSummaryWidge(GetAttendanceSummaryRequest query);

        [Get("/attendance-summary-term/unsubmited")]
        Task<ApiErrorResult<IEnumerable<GetAttendanceSummaryUnsubmitedResult>>> GetAttendanceSummaryUnsubmited(GetAttendanceSummaryUnsubmitedRequest query);

        [Get("/attendance-summary-term/pending")]
        Task<ApiErrorResult<IEnumerable<GetAttendanceSummaryPendingResult>>> GetAttendanceSummaryPending(GetAttendanceSummaryPendingRequest query);

        [Get("/attendance-summary-term/lesson-by-position")]
        Task<ApiErrorResult<IEnumerable<string>>> GetLessonByPosition(GetAttendanceSummaryUnsubmitedRequest query);

        [Get("/attendance-summary-term/pending-download")]
        Task<HttpResponseMessage> GetDownloadAttendanceSummaryPending(GetDownloadAttendanceSummaryPendingRequest query);
        #endregion

        #region Attendance summary detail (student)
        [Get("/attendance-summary-term-detail")]
        Task<ApiErrorResult<IEnumerable<GetAttendanceSummaryDetailResult>>> GetAttendanceSummaryDetail(GetAttendanceSummaryDetailRequest query);

        [Get("/attendance-summary-term-detail/widge")]
        Task<ApiErrorResult<GetAttendanceSummaryDetailWidgeResult>> GetAttendanceSummaryDetailWidge(GetAttendanceSummaryDetailRequest query);

        [Get("/attendance-summary-term/homeroom")]
        Task<ApiErrorResult<IEnumerable<GetAttendanceSummaryHomeroomResult>>> GetAttendanceSummaryHomeroom(GetAttendanceSummaryHomeroomRequest query);

        [Get("/attendance-summary-term/get-download-all-unexcused-absence-by-term")]
        Task<HttpResponseMessage> GetDownloadAllUnexcusedAbsenceByTerm(GetDownloadAllUnexcusedAbsenceByTermRequest query);

        [Get("/attendance-summary-term/download")]
        Task<HttpResponseMessage> GetDownloadAttendanceSummary(GetAttendanceSummaryDetailRequest query);
        
        [Get("/attendance-summary-term-detail/attendance-rate")]
        Task<ApiErrorResult<IEnumerable<GetAttendanceSummaryDetailAttendanceRateResult>>> GetAttendanceSummaryDetailAttendanceRate(GetAttendanceSummaryDetailAttendanceRateRequest query);

        [Get("/attendance-summary-term-detail/attendance-rate/download")]
        Task<HttpResponseMessage> GetAttendanceSummaryDetailAttendanceRateDownload(GetAttendanceSummaryDetailAttendanceRateRequest query);

        [Get("/attendance-summary-term-detail/unexcused-excused")]
        Task<ApiErrorResult<IEnumerable<GetAttendanceSummaryDetailUnexcusedExcusedResult>>> GetAttendanceSummaryDetailUnexcusedExcused(GetAttendanceSummaryDetailUnexcusedExcusedRequest query);

        [Get("/attendance-summary-term-detail/unexcused-excused/download")]
        Task<HttpResponseMessage> GetAttendanceSummaryDetailUnexcusedExcusedDownload(GetAttendanceSummaryDetailUnexcusedExcusedRequest query);

        [Get("/attendance-summary-term-detail/unexcused-excused/summary")]
        Task<ApiErrorResult<IEnumerable<GetAttendanceSummaryDetailUnexcusedExcusedSummaryResult>>> GetAttendanceSummaryDetailUnexcusedExcusedSummary(GetAttendanceSummaryDetailUnexcusedExcusedRequest query);

        [Get("/attendance-summary-term-detail/workhabit")]
        Task<ApiErrorResult<IEnumerable<GetAttendanceSummaryDetailWorkhabitResult>>> GetAttendanceSummaryDetailWorkhabit(GetAttendanceSummaryDetailWorkhabitRequest query);
        #endregion

        #region Attendance summary detail (level)
        [Get("/attendance-summary-term-detail-level")]
        Task<ApiErrorResult<IEnumerable<GetAttendanceSummaryDetailLevelResult>>> GetAttendanceSummaryDetailLevel(GetAttendanceSummaryDetailLevelRequest query);

        [Get("/attendance-summary-term-detail-level/widge")]
        Task<ApiErrorResult<GetAttendanceSummaryDetailLevelWidgeResult>> GetAttendanceSummaryDetailLevelWidge(GetAttendanceSummaryDetailLevelRequest query);

        [Get("/attendance-summary-term-detail-level/download")]
        Task<HttpResponseMessage> GetAttendanceSummaryDetailLevelDownload(GetAttendanceSummaryDetailLevelRequest query);
        #endregion

        #region Attendance summary detail (Subject)
        [Get("/attendance-summary-term-detail-subject")]
        Task<ApiErrorResult<IEnumerable<GetAttendanceSummaryDetailSubjectResult>>> GetAttendanceSummaryDetailSubject(GetAttendanceSummaryDetailSubjectRequest query);

        [Get("/attendance-summary-term-detail-subject/widge")]
        Task<ApiErrorResult<GetAttendanceSummaryDetailSubjectResult>> GetAttendanceSummaryDetailSubjectWidge(GetAttendanceSummaryDetailSubjectRequest query);

        [Get("/attendance-summary-term-detail-subject/download")]
        Task<HttpResponseMessage> GetAttendanceSummaryDetailSubjectDownload(GetAttendanceSummaryDetailSubjectRequest query);
        #endregion

        #region Attendance summary detail (School Day)
        [Get("/attendance-summary-term-detail-school-day")]
        Task<ApiErrorResult<IEnumerable<GetAttendanceSummaryDetailSchoolDayResult>>> GetAttendanceSummaryDetailSchoolDay(GetAttendanceSummaryDetailSchoolDayRequest query);

        [Get("/attendance-summary-term-detail-school-day/widge")]
        Task<ApiErrorResult<GetAttendanceSummaryDetailSchoolDayWidgeResult>> GetAttendanceSummaryDetailSchoolDayWidge(GetAttendanceSummaryDetailSchoolDayRequest query);

        [Get("/attendance-summary-term-detail-school-day/class-id-and-session")]
        Task<ApiErrorResult<IEnumerable<GetAttendanceSummaryDayClassIdAndSessionResult>>> GetAttendanceSummaryDayClassIdAndSession(GetAttendanceSummaryDayClassIdAndSessionRequest query);
        #endregion

        #region Dashboard
        [Get("/attendance-summary-term/dashboard")]
        Task<ApiErrorResult<GetAttendanceSummaryDashboardResult>> GetAttendanceSummaryDashboard(GetAttendanceSummaryDashboardRequest query);
        [Get("/attendance-summary-term/dashboard-parent")]
        Task<ApiErrorResult<IEnumerable<GetAttendanceSummaryDetailUnexcusedExcusedResult>>> GetAttendanceSummaryDashboardDetailParent(GetAttendanceSummaryDashboardDetailParentRequest query);
        #endregion

        #region Email
        [Get("/attendance-summary-term/email")]
        Task<ApiErrorResult> SendAttendanceSumamryEmail(SendAttendanceSumamryEmailRequest query);
        #endregion

        #region
        [Post("/attendance-summary-term/student-detail")]
        Task<ApiErrorResult<IEnumerable<GetStudentAttendanceSummaryTermResult>>> GetStudentAttendanceSummaryTerm([Body] GetStudentAttendanceSummaryTermRequest query);

        [Get("/attendance-summary-term/student-allterm-detail")]
        Task<ApiErrorResult<IEnumerable<GetStudentAttendanceSummaryAllTermResult>>> GetStudentAttendanceSummaryAllTerm(GetStudentAttendanceSummaryAllTermRequest query);

        [Get("/attendance-summary-term/student-attendance-rate-score-summary")]
        Task<ApiErrorResult<IEnumerable<GetStudentAttendanceRateForScoreSummaryResult>>> GetStudentAttendanceRateForScoreSummary(GetStudentAttendanceRateForScoreSummaryRequest query);
        #endregion
    }
}
