using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.Helper;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.ProjectTracking;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.SubmissionFlow;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IProjectInformation : IFnSchool
    {
        #region Helper
        [Get("/project-information/get-project-information-role-access")]
        Task<ApiErrorResult<GetProjectInformationRoleAccessResponse>> GetProjectInformationRoleAccess();
        #endregion

        #region Project Tracking
        [Get("/project-information/get-project-tracking-status")]
        Task<ApiErrorResult<IEnumerable<GetProjectTrackingStatusResponse>>> GetProjectTrackingStatus(GetProjectTrackingStatusRequest request);

        [Get("/project-information/get-project-tracking-year")]
        Task<ApiErrorResult<IEnumerable<GetProjectTrackingYearResponse>>> GetProjectTrackingYear(GetProjectTrackingYearRequest request);

        [Get("/project-information/get-project-tracking-section")]
        Task<ApiErrorResult<IEnumerable<GetProjectTrackingSectionResponse>>> GetProjectTrackingSection(GetProjectTrackingSectionRequest request);

        [Get("/project-information/get-project-tracking-phase")]
        Task<ApiErrorResult<IEnumerable<GetProjectTrackingPhaseResponse>>> GetProjectTrackingPhase(GetProjectTrackingPhaseRequest request);

        [Get("/project-information/get-project-tracking-pipelines")]
        Task<ApiErrorResult<IEnumerable<GetProjectTrackingPipelinesResponse>>> GetProjectTrackingPipelines(GetProjectTrackingPipelinesRequest request);

        [Post("/project-information/save-project-tracking-pipelines")]
        Task<ApiErrorResult> SaveProjectTrackingPipelines([Body] SaveProjectTrackingPipelinesRequest request);

        [Delete("/project-information/delete-project-tracking-pipelines")]
        Task<ApiErrorResult> DeleteProjectTrackingPipelines([Body] DeleteProjectTrackingPipelinesRequest request);

        [Get("/project-information/get-project-tracking-feedbacks")]
        Task<ApiErrorResult<IEnumerable<GetProjectTrackingFeedbacksResponse>>> GetProjectTrackingFeedbacks(GetProjectTrackingFeedbacksRequest request);

        [Post("/project-information/save-project-tracking-feedbacks")]
        Task<ApiErrorResult> SaveProjectTrackingFeedbacks([Body] SaveProjectTrackingFeedbacksRequest request);

        [Delete("/project-information/delete-project-tracking-feedbacks")]
        Task<ApiErrorResult> DeleteProjectTrackingFeedbacks([Body] DeleteProjectTrackingFeedbacksRequest request);

        [Get("/project-information/get-project-tracking-feature")]
        Task<ApiErrorResult<IEnumerable<GetProjectTrackingFeatureResponse>>> GetProjectTrackingFeature(GetProjectTrackingFeatureRequest request);

        [Get("/project-information/get-project-tracking-sub-feature")]
        Task<ApiErrorResult<IEnumerable<GetProjectTrackingSubFeatureResponse>>> GetProjectTrackingSubFeature(GetProjectTrackingSubFeatureRequest request);
        #endregion

        #region SPC List
        [Get("/project-information/get-spc-list")]
        Task<ApiErrorResult<IEnumerable<GetSPCListResponse>>> GetSPCList(GetSPCListRequest request);
        [Post("/project-information/save-spc")]
        Task<ApiErrorResult<IEnumerable<NameValueVm>>> SaveSPC([Body] SaveSPCRequest body);
        [Post("/project-information/delete-spc")]
        Task<ApiErrorResult<IEnumerable<NameValueVm>>> DeleteSPC([Body] DeleteSPCRequest body);
        #endregion
    }
}
