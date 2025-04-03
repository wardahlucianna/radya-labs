using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
using Refit;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IAttendanceSummary : IFnAttendance
    {
        [Get("/attendance-summary/by-range")]
        Task<ApiErrorResult<IEnumerable<SummaryResult>>> GetSummaryByRange(GetSummaryByRangeRequest query);

        [Get("/attendance-summary/by-period")]
        Task<ApiErrorResult<IEnumerable<SummaryResult>>> GetSummaryByPeriod(GetSummaryByPeriodRequest query);

        [Get("/attendance-summary/detail/by-student/by-range")]
        Task<ApiErrorResult<GetSummaryDetailResult<SummaryByStudentResult>>> GetSummaryDetailByStudentByRange(GetSummaryDetailByRangeRequest query);

        [Get("/attendance-summary/detail/by-student/by-period")]
        Task<ApiErrorResult<GetSummaryDetailResult<SummaryByStudentResult>>> GetSummaryDetailByStudentByPeriod(GetSummaryDetailByPeriodRequest query);

        [Get("/attendance-summary-detail-attendance-rate-by-student")]
        Task<ApiErrorResult<IEnumerable<GetAttendanceRateByStudentResult>>> GetAttendanceRateByStudent(GetAttendanceRateByStudentRequest query);

        [Get("/attendance-summary-detail-attendance-rate-by-student-term-day")]
        Task<ApiErrorResult<GetAttendanceRateByStudentTermDayResult>> GetAttendanceRateByStudentTermDay(GetAttendanceRateByStudentRequest query);

        [Get("/attendance-summary-detail-excused-absent")]
        Task<ApiErrorResult<IEnumerable<GetDetailExcusedAbsentStudentResult>>> GetDetailExcusedAbsentByStudent(GetDetailExcusedAbsentStudentRequest query);

        [Get("/attendance-summary-detail-excused-absent-term-day")]
        Task<ApiErrorResult<IEnumerable<GetDetailExcusedAbsentStudentResult>>> GetDetailExcusedAbsentByStudentTermDay(GetDetailExcusedAbsentStudentRequest query);

        [Get("/attendance-summary-detail-excused-absent-and-period")]
        Task<ApiErrorResult<IEnumerable<GetDetailExcusedAbsentStudentResult>>> GetDetailExcusedAbsentByStudentPeriod(GetDetailExcusedAbsentStudentAndPeriodRequest query);

        [Get("/attendance-summary-detail-excused-absent-and-period-term-day")]
        Task<ApiErrorResult<IEnumerable<GetDetailExcusedAbsentStudentResult>>> GetDetailExcusedAbsentByStudentPeriodTermDay(GetDetailExcusedAbsentStudentAndPeriodRequest query);

        [Get("/attendance-summary-detail-attendance-date-by-student")]
        Task<ApiErrorResult<GetDetailAttendanceToDateByStudentResult>> GetDetailAttendanceToDateByStudent(GetDetailAttendanceToDateByStudentRequest query);

        [Get("/attendance-summary-detail-workhabit-by-student")]
        Task<ApiErrorResult<IEnumerable<GetDetailAttendanceWorkhabitByStudentResult>>> GetDetailWorkhabitByStudent(GetDetailAttendanceWorkhabitByStudentRequest query);
        [Get("/attendance-summary-detail-workhabit-by-student-term-day")]
        Task<ApiErrorResult<IEnumerable<GetDetailAttendanceWorkhabitByStudentResult>>> GetDetailWorkhabitByStudentTermDay(GetDetailAttendanceWorkhabitByStudentRequest query);

        [Get("/attendance-summary-detail-workhabit-by-student-and-period")]
        Task<ApiErrorResult<IEnumerable<GetDetailAttendanceWorkhabitByStudentAndPeriodResult>>> GetDetailWorkhabitByStudentPeriod(GetDetailAttendanceWorkhabitByStudentAndPeriodRequest query);
        [Get("/attendance-summary-detail-workhabit-by-student-and-period-term-day")]
        Task<ApiErrorResult<IEnumerable<GetDetailAttendanceWorkhabitByStudentAndPeriodResult>>> GetDetailWorkhabitByStudentPeriodTermDay(GetDetailAttendanceWorkhabitByStudentAndPeriodRequest query);

        [Get("/attendance-summary-attendance-and-workhabit-by-level")]
        Task<ApiErrorResult<GetAttendanceAndWorkhabitByLevelResult>> GetAttendanceAndWorkhabitByLevel(GetAttendanceAndWorkhabitByLevelRequest query);

        [Get("/attendance-summary/detail/by-school-day/by-range")]
        Task<ApiErrorResult<GetSummaryDetailResult<SummaryBySchoolDayResult>>> GetSummaryDetailBySchoolDayByRange(GetSummaryDetailByRangeRequest query);

        [Get("/attendance-summary/detail/by-school-day/by-period")]
        Task<ApiErrorResult<GetSummaryDetailResult<SummaryBySchoolDayResult>>> GetSummaryDetailBySchoolDayByPeriod(GetSummaryDetailByPeriodRequest query);


        #region Summary By Level By Period And Range Term Day And Session
        [Get("/attendance-summary/detail/by-level/by-range")]
        Task<ApiErrorResult<GetSummaryDetailResult<SummaryByLevelResult>>> GetSummaryDetailByLevelByRange(GetSummaryDetailByRangeRequest query);

        [Get("/attendance-summary/detail/by-level/by-range/term-day")]
        Task<ApiErrorResult<GetSummaryDetailResult<SummaryByLevelResult>>> GetSummaryDetailByLevelByRangeTermDay(GetSummaryDetailByRangeRequest query);

        [Get("/attendance-summary/detail/by-level/by-period")]
        Task<ApiErrorResult<GetSummaryDetailResult<SummaryByLevelResult>>> GetSummaryDetailByLevelByPeriod(GetSummaryDetailByPeriodRequest query);

        [Get("/attendance-summary/detail/by-level/by-period/term-day")]
        Task<ApiErrorResult<GetSummaryDetailResult<SummaryByLevelResult>>> GetSummaryDetailByLevelByPeriodTermDay(GetSummaryDetailByPeriodRequest query);

        [Get("/attendance-summary/detail/unsubmitted/by-level/by-period/")]
        Task<ApiErrorResult<IEnumerable<GetSummaryDetailUnsubmittedByLevelByPeriodResponse>>> GetSummaryDetailUnsubmittedByLevelByPeriod(GetSummaryDetailUnsubmittedByLevelByPeriodRequest query);
        [Get("/attendance-summary/detail/unsubmitted/by-level/by-period/term-day")]
        Task<ApiErrorResult<IEnumerable<GetSummaryDetailUnsubmittedByLevelByPeriodTermDayResponse>>> GetSummaryDetailUnsubmittedByLevelByPeriodTermDay(GetSummaryDetailUnsubmittedByLevelByPeriodRequest query);
        [Get("/attendance-summary/detail/unsubmitted/by-level/by-range/")]
        Task<ApiErrorResult<IEnumerable<GetSummaryDetailUnsubmittedByLevelByPeriodResponse>>> GetSummaryDetailUnsubmittedByLevelByRange(GetSummaryDetailUnsubmittedByLevelByRangeRequest query);
        [Get("/attendance-summary/detail/unsubmitted/by-level/by-range/term-day")]
        Task<ApiErrorResult<IEnumerable<GetSummaryDetailUnsubmittedByLevelByPeriodTermDayResponse>>> GetSummaryDetailUnsubmittedByLevelByRangeTermDay(GetSummaryDetailUnsubmittedByLevelByRangeRequest query);


        [Get("/attendance-summary/detail/pending/by-level/by-period/")]
        Task<ApiErrorResult<IEnumerable<GetSummaryDetailUnsubmittedByLevelByPeriodResponse>>> GetSummaryDetailPendingByLevelByPeriod(GetSummaryDetailUnsubmittedByLevelByPeriodRequest query);
        [Get("/attendance-summary/detail/pending/by-level/by-period/term-day")]
        Task<ApiErrorResult<IEnumerable<GetSummaryDetailUnsubmittedByLevelByPeriodTermDayResponse>>> GetSummaryDetailPendingByLevelByPeriodTermDay(GetSummaryDetailUnsubmittedByLevelByPeriodRequest query);
        [Get("/attendance-summary/detail/pending/by-level/by-range/")]
        Task<ApiErrorResult<IEnumerable<GetSummaryDetailUnsubmittedByLevelByPeriodResponse>>> GetSummaryDetailPendingByLevelByRange(GetSummaryDetailUnsubmittedByLevelByRangeRequest query);
        [Get("/attendance-summary/detail/pending/by-level/by-range/term-day")]
        Task<ApiErrorResult<IEnumerable<GetSummaryDetailUnsubmittedByLevelByPeriodTermDayResponse>>> GetSummaryDetailPendingByLevelByRangeTermDay(GetSummaryDetailUnsubmittedByLevelByRangeRequest query);
        #endregion


        [Get("/attendance-summary/detail/by-subject/by-range")]
        Task<ApiErrorResult<GetSummaryDetailResult<SummaryBySubjectResult>>> GetSummaryDetailBySubjectByRange(GetSummaryDetailByRangeRequest query);

        [Get("/attendance-summary/detail/by-subject/by-period")]
        Task<ApiErrorResult<GetSummaryDetailResult<SummaryBySubjectResult>>> GetSummaryDetailBySubjectByPeriod(GetSummaryDetailByPeriodRequest query);

        [Get("/attendance-summary/detail/widget/by-range")]
        Task<ApiErrorResult<SummaryDetailWidgetResult>> GetSummaryDetailWidgetByRange(GetSummaryDetailByRangeRequest query);

        [Get("/attendance-summary/detail/widget/by-period")]
        Task<ApiErrorResult<SummaryDetailWidgetResult>> GetSummaryDetailWidgetByPeriod(GetSummaryDetailByPeriodRequest query);

        [Get("/attendance-summary-overall-attendance-rate-by-student")]
        Task<ApiErrorResult<IEnumerable<GetOverallAttendanceRateByStudentResult>>> GetOverallAttendanceRateByStudent(GetOverallAttendanceRateByStudentRequest query);
                
        [Get("/attendance-summary-overall-term-attendance-rate-by-student")]
        Task<ApiErrorResult<IEnumerable<GetOverallTermAttendanceRateByStudentResult>>> GetOverallTermAttendanceRateByStudent(GetOverallTermAttendanceRateByStudentRequest query);
        
        [Get("/attendance-summary-avaiability-position")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetTeacherAvaiabilityPosition(GetAvailabilityPositionByUserRequest query);

        #region Summary Unsubmitted and Pending By Period And Range Term Day And Session
        [Get("/attendance-summary-unsubmitted-by-range")]
        Task<ApiErrorResult<IEnumerable<UnresolvedAttendanceGroupResult>>> GetSummaryUnsubmittedByRange(GetSummaryUnsubmittedByRangeRequest query);
        [Get("/attendance-summary-unsubmitted-by-range-term-day")]
        Task<ApiErrorResult<IEnumerable<UnresolvedAttendanceTermDayResult>>> GetSummaryUnsubmittedByRangeTermDay(GetSummaryUnsubmittedByRangeRequest query);
        [Get("/attendance-summary-unsubmitted-by-period")]
        Task<ApiErrorResult<IEnumerable<UnresolvedAttendanceGroupResult>>> GetSummaryUnsubmittedByPeriod(GetSummaryUnsubmittedByPeriodRequest query);
        [Get("/attendance-summary-unsubmitted-by-period-term-day")]
        Task<ApiErrorResult<IEnumerable<UnresolvedAttendanceTermDayResult>>> GetSummaryUnsubmittedByPeriodTermDay(GetSummaryUnsubmittedByPeriodRequest query);
        [Get("/attendance-summary-pending-by-range")]
        Task<ApiErrorResult<IEnumerable<UnresolvedAttendanceGroupResult>>> GetSummaryPendingByRange(GetSummaryUnsubmittedByRangeRequest query);
        [Get("/attendance-summary-pending-by-range-term-day")]
        Task<ApiErrorResult<IEnumerable<UnresolvedAttendanceTermDayResult>>> GetSummaryPendingByRangeTermDay(GetSummaryUnsubmittedByRangeRequest query);
        [Get("/attendance-summary-pending-by-period")]
        Task<ApiErrorResult<IEnumerable<UnresolvedAttendanceGroupResult>>> GetSummaryPendingByPeriod(GetSummaryUnsubmittedByPeriodRequest query);
        [Get("/attendance-summary-pending-by-period-term-day")]
        Task<ApiErrorResult<IEnumerable<UnresolvedAttendanceTermDayResult>>> GetSummaryPendingdByPeriodTermDay(GetSummaryUnsubmittedByPeriodRequest query);
        #endregion

        #region Summary for Dashboard
        [Get("/attendance-summary/student")]
        Task<ApiErrorResult<GetStudentAttendanceSummaryResult>> GetStudentAttendanceSummary(GetStudentAttendanceSummaryRequest query);
        [Get("/attendance-summary/student/day")]
        Task<ApiErrorResult<IEnumerable<GetStudentAttendanceSummaryDayDetailResult>>> GetStudentAttendanceSummaryDayDetail(GetStudentAttendanceSummaryDetailRequest query);
        [Get("/attendance-summary/student/session")]
        Task<ApiErrorResult<IEnumerable<GetStudentAttendanceSummarySessionDetailResult>>> GetStudentAttendanceSummarySessionDetail(GetStudentAttendanceSummaryDetailRequest query);
        [Get("/attendance-summary/student/workhabit/day")]
        Task<ApiErrorResult<IEnumerable<GetStudentWorkhabitDayDetailResult>>> GetStudentWorkhabitDayDetail(GetStudentWorkhabitDetailRequest query);
        [Get("/attendance-summary/student/workhabit/session")]
        Task<ApiErrorResult<IEnumerable<GetStudentWorkhabitSessionDetailResult>>> GetStudentWorkhabitSessionDetail(GetStudentWorkhabitDetailRequest query);
        #endregion

        #region Get Download All Unexcused Absence
        [Get("/attendance-summary/get-download-all-unexcused-absence-by-range")]
        Task<HttpResponseMessage> GetDownloadAllUnexcusedAbsenceByRange(GetDownloadAllUnexcusedAbsenceByRangeRequest query);

        [Get("/attendance-summary/get-download-all-unexcused-absence-by-period")]
        Task<HttpResponseMessage> GetDownloadAllUnexcusedAbsenceByPeriod(GetDownloadAllUnexcusedAbsenceByPeriodRequest query);
        #endregion

    }
}
