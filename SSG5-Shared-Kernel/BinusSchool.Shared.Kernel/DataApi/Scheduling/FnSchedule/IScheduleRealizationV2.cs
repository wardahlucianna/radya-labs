using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IScheduleRealizationV2 : IFnSchedule
    {

        [Get("/schedule/schedule-realizationv2/get-session-venue-for-schedule-realization")]
        Task<ApiErrorResult<IEnumerable<GetSessionVenueForScheduleRealizationResult>>> GetSessionVenueForScheduleRealization(GetSessionVenueForScheduleRealizationRequest query);

        [Get("/schedule/schedule-realizationv2/get-list-schedule-realization")]
        Task<ApiErrorResult<IEnumerable<GetListScheduleRealizationV2Result>>> GetListScheduleRealizationV2(GetListScheduleRealizationV2Request query);

        [Post("/schedule/schedule-realizationv2/save-schedule-realization")]
        Task<ApiErrorResult> SaveScheduleRealizationV2([Body] SaveScheduleRealizationV2Request body);

        [Post("/schedule/schedule-realizationv2/undo-schedule-realization")]
        Task<ApiErrorResult> UndoScheduleRealizationV2([Body] UndoScheduleRealizationV2Request body);

        [Post("/schedule/schedule-realizationv2/reset-schedule-realization")]
        Task<ApiErrorResult> ResetScheduleRealizationV2([Body] ResetScheduleRealizationRequest body);

        [Post("/schedule/schedule-realizationv2/undo-schedule-realization-by-teacher")]
        Task<ApiErrorResult> UndoScheduleRealizationByTeacherV2([Body] UndoScheduleRealizationByTeacherV2Request body);

        [Post("/schedule/schedule-realizationv2/reset-schedule-realization-by-teacher")]
        Task<ApiErrorResult> ResetScheduleRealizationByTeacherV2([Body] ResetScheduleRealizationByTeacherRequest body);

        [Get("/schedule/schedule-realizationv2/get-class-id-by-teacher-date")]
        Task<ApiErrorResult<IEnumerable<GetClassIdByTeacherDateV2Result>>> GetClassIdByTeacherDateV2(GetClassIdByTeacherDateV2Request query);

        [Get("/schedule/schedule-realizationv2/get-list-schedule-realization-by-teacher")]
        Task<ApiErrorResult<IEnumerable<GetListScheduleRealizationByTeacherV2Result>>> GetListScheduleRealizationByTeacherV2(GetListScheduleRealizationByTeacherV2Request query);

        [Post("/schedule/schedule-realizationv2/save-schedule-realization-by-teacher")]
        Task<ApiErrorResult> SaveScheduleRealizationByTeacherV2([Body] SaveScheduleRealizationByTeacherV2Request body);

        [Get("/schedule/schedule-realizationv2/get-level-grade-for-substitution")]
        Task<ApiErrorResult<IEnumerable<GetLevelGradeForSubstitutionResult>>> GetLevelGradeForSubstitution(GetLevelGradeForSubstitutionRequest query);

        [Get("/schedule/schedule-realizationv2/get-teacher-for-substitution")]
        Task<ApiErrorResult<IEnumerable<GetTeacherForSubstitutionV2Result>>> GetTeacherForSubstitutionV2(GetTeacherForSubstitutionV2Request query);

        [Get("/schedule/schedule-realizationv2/get-session-venue-for-substitution")]
        Task<ApiErrorResult<IEnumerable<GetSessionVenueForSubstitutionResult>>> GetSessionVenueForSubstitution(GetSessionVenueForSubstitutionRequest query);

        [Get("/schedule/schedule-realizationv2/get-list-substitution-report")]
        Task<ApiErrorResult<IEnumerable<GetListSubstitutionReportV2Result>>> GetListSubstitutionReportV2(GetListSubstitutionReportV2Request query);

        [Post("/schedule/schedule-realizationv2/send-email-for-cancel-class")]
        Task<ApiErrorResult> SendEmailForCancelClassV2([Body] SendEmailForCancelClassV2Request body);

        [Get("/schedule/schedule-realizationv2/download-substitution-report")]
        Task<HttpResponseMessage> DownloadSubstitutionReportV2(DownloadSubstitutionReportV2Request query);

        [Get("/schedule/schedule-realizationv2/get-download-schedule-realization")]
        Task<ApiErrorResult<IEnumerable<GetDownloadScheduleRealizationResult>>> GetDownloadScheduleRealizationV2(GetDownloadScheduleRealizationRequest query);

        [Get("/schedule/schedule-realizationv2/check-teacher-on-schedule-realization")]
        Task<ApiErrorResult<CheckTeacherOnScheduleRealizationV2Result>> CheckTeacherOnScheduleRealizationV2(CheckTeacherOnScheduleRealizationV2Request query);
    }
}
