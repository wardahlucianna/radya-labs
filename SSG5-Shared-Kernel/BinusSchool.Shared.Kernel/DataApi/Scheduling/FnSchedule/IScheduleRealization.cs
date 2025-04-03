using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IScheduleRealization : IFnSchedule
    {
        [Get("/schedule/schedule-realization/get-teacher-by-date-ay")]
        Task<ApiErrorResult<IEnumerable<GetTeacherByDateAYResult>>> GetTeacherByDateAY(GetTeacherByDateAYRequest query);

        [Get("/schedule/schedule-realization/get-session-by-teacher-date")]
        Task<ApiErrorResult<IEnumerable<GetSessionByTeacherDateResult>>> GetSessionByTeacherDate(GetSessionByTeacherDateReq query);

        [Get("/schedule/schedule-realization/get-venue-by-teacher-date")]
        Task<ApiErrorResult<IEnumerable<GetVenueByTeacherDateResult>>> GetVenueByTeacherDateRequest(GetVenueByTeacherDateRequest query);

        [Get("/schedule/schedule-realization/get-list-schedule-realization")]
        Task<ApiErrorResult<IEnumerable<GetListScheduleRealizationResult>>> GetListScheduleRealization(GetListScheduleRealizationRequest query);

        [Post("/schedule/schedule-realization/save-schedule-realization")]
        Task<ApiErrorResult> SaveScheduleRealization([Body] SaveScheduleRealizationRequest body);

        [Get("/schedule/schedule-realization/get-class-id-by-teacher-date")]
        Task<ApiErrorResult<IEnumerable<GetClassIdByTeacherDateResult>>> GetClassIdByTeacherDate(GetClassIdByTeacherDateRequest query);

        [Get("/schedule/schedule-realization/get-list-schedule-realization-by-teacher")]
        Task<ApiErrorResult<IEnumerable<GetListScheduleRealizationByTeacherResult>>> GetListScheduleRealizationByTeacher(GetListScheduleRealizationByTeacherRequest query);

        [Post("/schedule/schedule-realization/save-schedule-realization-by-teacher")]
        Task<ApiErrorResult> SaveScheduleRealizationByTeacher([Body] SaveScheduleRealizationByTeacherRequest body);

        [Get("/schedule/schedule-realization/get-level-for-substitution")]
        Task<ApiErrorResult<IEnumerable<GetLevelForSubstitutionResult>>> GetLevelForSubstitution(GetLevelForSubstitutionRequest query);

        [Get("/schedule/schedule-realization/get-grade-for-substitution")]
        Task<ApiErrorResult<IEnumerable<GetGradeForSubstitutionResult>>> GetGradeForSubstitution(GetGradeForSubstitutionRequest query);

        [Get("/schedule/schedule-realization/get-teacher-for-substitution")]
        Task<ApiErrorResult<IEnumerable<GetTeacherForSubstitutionResult>>> GetTeacherForSubstitution(GetTeacherForSubstitutionRequest query);

        [Get("/schedule/schedule-realization/get-session-for-substitution")]
        Task<ApiErrorResult<IEnumerable<GetSessionForSubstitutionResult>>> GetSessionForSubstitution(GetSessionForSubstitutionRequest query);

        [Get("/schedule/schedule-realization/get-venue-for-substitution")]
        Task<ApiErrorResult<IEnumerable<GetVenueForSubstitutionResult>>> GetVenueForSubstitution(GetVenueForSubstitutionRequest query);

        [Get("/schedule/schedule-realization/get-list-substitution-report")]
        Task<ApiErrorResult<IEnumerable<GetListSubstitutionReportResult>>> GetListSubstitutionReport(GetListSubstitutionReportRequest query);

        [Get("/schedule/schedule-realization/get-setting-email-schedule-realization")]
        Task<ApiErrorResult<GetSettingEmailScheduleRealizationResult>> GetSettingEmailScheduleRealization(GetSettingEmailScheduleRealizationRequest query);

        [Post("/schedule/schedule-realization/save-setting-email-schedule-realization")]
        Task<ApiErrorResult> SaveSettingEmailScheduleRealization([Body] SaveSettingEmailScheduleRealizationRequest body);

        [Post("/schedule/schedule-realization/send-email-for-cancel-class")]
        Task<ApiErrorResult> SendEmailForCancelClass([Body] SendEmailForCancelClassRequest body);

        [Get("/schedule/schedule-realization/download-substitution-report")]
        Task<HttpResponseMessage> DownloadSubstitutionReport(DownloadSubstitutionReportRequest query);

        [Get("/schedule/schedule-realization/get-student-attendance")]
        Task<ApiErrorResult<IEnumerable<GetStudentAttendanceResult>>> GetStudentAttendance(GetStudentAttendanceRequest query);

        [Get("/schedule/schedule-realization/get-download-schedule-realization")]
        Task<ApiErrorResult<IEnumerable<GetDownloadScheduleRealizationResult>>> GetDownloadScheduleRealization(GetDownloadScheduleRealizationRequest query);

        [Get("/schedule/schedule-realization/check-teacher-on-schedule-realization")]
        Task<ApiErrorResult<CheckTeacherOnScheduleRealizationResult>> CheckTeacherOnScheduleRealization(CheckTeacherOnScheduleRealizationRequest query);

    }
}
