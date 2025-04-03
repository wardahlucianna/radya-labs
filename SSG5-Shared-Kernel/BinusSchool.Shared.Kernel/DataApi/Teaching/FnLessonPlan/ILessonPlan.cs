using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BinusSchool.Data.Api.Teaching.FnLessonPlan
{
    public interface ILessonPlan : IFnLessonPlan
    {
        [Get("/lesson-plan/approval/status")]
        Task<ApiErrorResult<IEnumerable<GetLessonPlanApprovalStatusResult>>> DeleteWeekSetting(GetLessonPlanApprovalStatusRequest request);

        [Get("/lesson-plan")]
        Task<ApiErrorResult<IEnumerable<GetLessonPlanResult>>> GetLessonPlan(GetLessonPlanRequest request);

        [Get("/lesson-plan/documents")]
        Task<ApiErrorResult<GetLessonPlanDocumentListResult>> GetLessonPlanDocumentList(GetLessonPlanDocumentListRequest request);

        [Post("/lesson-plan-document")]
        Task<ApiErrorResult> AddLessonPlanDocument([Body] AddLessonPlanDocumentRequest body);

        [Get("/lesson-plan-document-detail")]
        Task<ApiErrorResult<GetDetailLessonPlanDocumentResult>> GetDetailLessonPlanDocument(GetDetailLessonPlanDocumentRequest request);

        [Get("/lesson-plan-information-detail")]
        Task<ApiErrorResult<GetDetailLessonPlanInformationResult>> GetDetailLessonPlanInformation(GetDetailLessonPlanInformationRequest request);

        [Post("/lesson-plan/approval")]
        Task<ApiErrorResult> SetLessonPlanApprovalStatus([Body] SetLessonPlanApprovalStatusRequest request);

        [Get("/lesson-plan/approval/subject")]
        Task<ApiErrorResult<IEnumerable<GetSubjectLessonPlanApprovalResult>>> GetSubjectLessonPlanApproval(GetSubjectLessonPlanApprovalRequest request);

        [Get("/lesson-plan/summary/detail")]
        Task<ApiErrorResult<GetLessonPlanSummaryDetailResult>> GetLessonPlanSummaryDetail(GetLessonPlanSummaryDetailRequest request);

        [Get("/lesson-plan-approval-setting")]
        Task<ApiErrorResult<IEnumerable<GetLessonPlanApprovalSettingResult>>> GetLessonPlanApprovalSetting(GetLessonPlanApprovalSettingRequest request);

        [Post("/lesson-plan-approval-setting")]
        Task<ApiErrorResult> SetLessonPlanApprovalSetting(SetLessonPlanApprovalSettingRequest request);

        [Get("/lesson-plan-approval")]
        Task<ApiErrorResult<IEnumerable<GetLessonPlanApprovalResult>>> GetLessonPlanApproval(GetLessonPlanApprovalRequest request);

        [Get("/lesson-plan-summary")]
        Task<ApiErrorResult<IEnumerable<GetLessonPlanSummaryResult>>> GetLessonPlanSummary(GetLessonPlanSummaryRequest request);

        [Get("/lesson-plan-summary/download-lesson-plan-summary")]
        Task<ApiErrorResult<string>> GetDownloadLessonPlanSummary(GetDownloadLessonPlanSummaryRequest request);

        [Post("/lesson-plan-summary/lesson-plan-blob")]
        Task<ApiErrorResult> LessonPlanBlob(DeleteLessonPlanBlobRequest request);

        [Get("/lesson-plan/get-subject-for-lesson-plan")]
        Task<ApiErrorResult<IEnumerable<GetSubjectForLessonPlanResult>>> GetSubjectForLessonPlan(GetSubjectForLessonPlanRequest request);

        [Get("/lesson-plan/level-by-position")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> LessonPlanLevelByPosition(LessonPlanLevelByPositionRequest query);

        [Get("/lesson-plan/grade-by-position")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> LessonPlanGradeByPosition(LessonPlanGradeByPositionRequest query);
        [Get("/lesson-plan/lesson-plan-approver-setting")]
        Task<ApiErrorResult<IEnumerable<GetLessonPlanApproverSettingResult>>> GetLessonPlanApproverSetting(GetLessonPlanApproverSettingRequest query);

        [Post("/lesson-plan/lesson-plan-approver-setting")]
        Task<ApiErrorResult> AddLessonPlanApproverSetting(AddLessonPlanApproverSettingRequest query);

        [Get("/lesson-plan/grade-by-position-v2")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> LessonPlanGradeByPositionV2(LessonPlanGradeByPositionV2Request query);
    }
}
