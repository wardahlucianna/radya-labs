using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using BinusSchool.School.FnSchool.ProjectInformation.ProjectTracking;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs;
using Microsoft.OpenApi.Models;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.ProjectTracking;
using BinusSchool.School.FnSchool.ProjectInformation.SubmissionFlow;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.SubmissionFlow;
using BinusSchool.School.FnSchool.ProjectInformation.Helper;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.Helper;

namespace BinusSchool.School.FnSchool.ProjectInformation
{
    public class ProjectInformationEndpoint
    {
        private const string _route = "project-information";
        private const string _tag = "Project Information";

        #region Helper
        private readonly GetProjectInformationRoleAccessHandler _getProjectInformationRoleAccessHandler;
        #endregion

        #region Project Tracking
        private readonly GetProjectTrackingStatusHandler _getProjectTrackingStatusHandler;
        private readonly GetProjectTrackingYearHandler _getProjectTrackingYearHandler;
        private readonly GetProjectTrackingSectionHandler _getProjectTrackingSectionHandler;
        private readonly GetProjectTrackingPhaseHandler _getProjectTrackingPhaseHandler;
        private readonly GetProjectTrackingPipelinesHandler _getProjectTrackingPipelinesHandler;
        private readonly SaveProjectTrackingPipelinesHandler _saveProjectTrackingPipelinesHandler;
        private readonly DeleteProjectTrackingPipelinesHandler _deleteProjectTrackingPipelinesHandler;
        private readonly GetProjectTrackingFeedbacksHandler _getProjectTrackingFeedbacksHandler;
        private readonly SaveProjectTrackingFeedbacksHandler _saveProjectTrackingFeedbacksHandler;
        private readonly DeleteProjectTrackingFeedbacksHandler _deleteProjectTrackingFeedbacksHandler;
        private readonly GetProjectTrackingFeatureHandler _getProjectTrackingFeatureHandler;
        private readonly GetProjectTrackingSubFeatureHandler _getProjectTrackingSubFeatureHandler;
        #endregion

        #region SPC List
        private readonly GetSPCListHandler _getSPCListHandler;
        private readonly SaveSPCHandler _saveSPCHandler;
        private readonly DeleteSPCHandler _deleteSPCHandler;
        #endregion

        public ProjectInformationEndpoint(
            // Helper
            GetProjectInformationRoleAccessHandler getProjectInformationRoleAccessHandler,

            // Project Tracking
            GetProjectTrackingStatusHandler getProjectTrackingStatusHandler,
            GetProjectTrackingYearHandler getProjectTrackingYearHandler,
            GetProjectTrackingSectionHandler getProjectTrackingSectionHandler,
            GetProjectTrackingPhaseHandler getProjectTrackingPhaseHandler,
            GetProjectTrackingPipelinesHandler getProjectTrackingPipelinesHandler,
            SaveProjectTrackingPipelinesHandler saveProjectTrackingPipelinesHandler,
            DeleteProjectTrackingPipelinesHandler deleteProjectTrackingPipelinesHandler,
            GetProjectTrackingFeedbacksHandler getProjectTrackingFeedbacksHandler,
            SaveProjectTrackingFeedbacksHandler saveProjectTrackingFeedbacksHandler,
            DeleteProjectTrackingFeedbacksHandler deleteProjectTrackingFeedbacksHandler,
            GetProjectTrackingFeatureHandler getProjectTrackingFeatureHandler,
            GetProjectTrackingSubFeatureHandler getProjectTrackingSubFeatureHandler,

            // SPC List
            GetSPCListHandler getSPCListHandler,
            SaveSPCHandler saveSPCHandler,
            DeleteSPCHandler deleteSPCHandler)
        {
            // Helper
            _getProjectInformationRoleAccessHandler = getProjectInformationRoleAccessHandler;

            // Project Tracking
            _getProjectTrackingStatusHandler = getProjectTrackingStatusHandler;
            _getProjectTrackingYearHandler = getProjectTrackingYearHandler;
            _getProjectTrackingSectionHandler = getProjectTrackingSectionHandler;
            _getProjectTrackingPhaseHandler = getProjectTrackingPhaseHandler;
            _getProjectTrackingPipelinesHandler = getProjectTrackingPipelinesHandler;
            _saveProjectTrackingPipelinesHandler = saveProjectTrackingPipelinesHandler;
            _deleteProjectTrackingPipelinesHandler = deleteProjectTrackingPipelinesHandler;
            _getProjectTrackingFeedbacksHandler = getProjectTrackingFeedbacksHandler;
            _saveProjectTrackingFeedbacksHandler = saveProjectTrackingFeedbacksHandler;
            _deleteProjectTrackingFeedbacksHandler = deleteProjectTrackingFeedbacksHandler;
            _getProjectTrackingFeatureHandler = getProjectTrackingFeatureHandler;
            _getProjectTrackingSubFeatureHandler = getProjectTrackingSubFeatureHandler;

            // SPC List
            _getSPCListHandler = getSPCListHandler;
            _saveSPCHandler = saveSPCHandler;
            _deleteSPCHandler = deleteSPCHandler;
        }

        #region Helper
        [FunctionName(nameof(ProjectInformationEndpoint.GetProjectInformationRoleAccess))]
        [OpenApiOperation(tags: _tag, Summary = "Get Project Information Role Access")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetProjectInformationRoleAccessResponse))]
        public Task<IActionResult> GetProjectInformationRoleAccess(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-project-information-role-access")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getProjectInformationRoleAccessHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region Project Tracking
        [FunctionName(nameof(ProjectInformationEndpoint.GetProjectTrackingStatus))]
        [OpenApiOperation(tags: _tag, Summary = "Get Project Tracking Status")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetProjectTrackingStatusRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetProjectTrackingStatusResponse>))]
        public Task<IActionResult> GetProjectTrackingStatus(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-project-tracking-status")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getProjectTrackingStatusHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ProjectInformationEndpoint.GetProjectTrackingYear))]
        [OpenApiOperation(tags: _tag, Summary = "Get Project Tracking Year")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetProjectTrackingYearRequest.Type), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetProjectTrackingYearRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetProjectTrackingYearResponse>))]
        public Task<IActionResult> GetProjectTrackingYear(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-project-tracking-year")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getProjectTrackingYearHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ProjectInformationEndpoint.GetProjectTrackingSection))]
        [OpenApiOperation(tags: _tag, Summary = "Get Project Tracking Section")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetProjectTrackingSectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetProjectTrackingSectionResponse>))]
        public Task<IActionResult> GetProjectTrackingSection(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-project-tracking-section")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getProjectTrackingSectionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ProjectInformationEndpoint.GetProjectTrackingPhase))]
        [OpenApiOperation(tags: _tag, Summary = "Get Project Tracking Phase")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetProjectTrackingPhaseRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetProjectTrackingPhaseResponse>))]
        public Task<IActionResult> GetProjectTrackingPhase(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-project-tracking-phase")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getProjectTrackingPhaseHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ProjectInformationEndpoint.GetProjectTrackingPipelines))]
        [OpenApiOperation(tags: _tag, Summary = "Get Project Tracking Pipelines")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetProjectTrackingPipelinesRequest.Year), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetProjectTrackingPipelinesRequest.IdSection), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetProjectTrackingPipelinesRequest.IdStatus), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetProjectTrackingPipelinesRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetProjectTrackingPipelinesRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetProjectTrackingPipelinesRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetProjectTrackingPipelinesRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetProjectTrackingPipelinesRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetProjectTrackingPipelinesRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetProjectTrackingPipelinesRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetProjectTrackingPipelinesResponse>))]
        public Task<IActionResult> GetProjectTrackingPipelines(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-project-tracking-pipelines")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getProjectTrackingPipelinesHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ProjectInformationEndpoint.SaveProjectTrackingPipelines))]
        [OpenApiOperation(tags: _tag, Summary = "Save Project Tracking Pipelines")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveProjectTrackingPipelinesRequest))]
        public Task<IActionResult> SaveProjectTrackingPipelines(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-project-tracking-pipelines")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _saveProjectTrackingPipelinesHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ProjectInformationEndpoint.DeleteProjectTrackingPipelines))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Project Tracking Pipelines")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteProjectTrackingPipelinesRequest))]
        public Task<IActionResult> DeleteProjectTrackingPipelines(
           [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-project-tracking-pipelines")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _deleteProjectTrackingPipelinesHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ProjectInformationEndpoint.GetProjectTrackingFeedbacks))]
        [OpenApiOperation(tags: _tag, Summary = "Get Project Tracking Feedbacks")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetProjectTrackingFeedbacksRequest.Year), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetProjectTrackingFeedbacksRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetProjectTrackingFeedbacksRequest.IdStatus), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetProjectTrackingFeedbacksRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetProjectTrackingFeedbacksRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetProjectTrackingFeedbacksRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetProjectTrackingFeedbacksRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetProjectTrackingFeedbacksRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetProjectTrackingFeedbacksRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetProjectTrackingFeedbacksRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetProjectTrackingFeedbacksResponse>))]
        public Task<IActionResult> GetProjectTrackingFeedbacks(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-project-tracking-feedbacks")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getProjectTrackingFeedbacksHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ProjectInformationEndpoint.SaveProjectTrackingFeedbacks))]
        [OpenApiOperation(tags: _tag, Summary = "Save Project Tracking Feedbacks")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveProjectTrackingFeedbacksRequest))]
        public Task<IActionResult> SaveProjectTrackingFeedbacks(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-project-tracking-feedbacks")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _saveProjectTrackingFeedbacksHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ProjectInformationEndpoint.DeleteProjectTrackingFeedbacks))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Project Tracking Feedbacks")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteProjectTrackingFeedbacksRequest))]
        public Task<IActionResult> DeleteProjectTrackingFeedbacks(
           [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-project-tracking-feedbacks")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _deleteProjectTrackingFeedbacksHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ProjectInformationEndpoint.GetProjectTrackingFeature))]
        [OpenApiOperation(tags: _tag, Summary = "Get Project Tracking Feature")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetProjectTrackingFeatureRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetProjectTrackingFeatureResponse>))]
        public Task<IActionResult> GetProjectTrackingFeature(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-project-tracking-feature")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getProjectTrackingFeatureHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ProjectInformationEndpoint.GetProjectTrackingSubFeature))]
        [OpenApiOperation(tags: _tag, Summary = "Get Project Tracking Sub Feature")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetProjectTrackingSubFeatureRequest.IdFeature), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetProjectTrackingSubFeatureRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetProjectTrackingSubFeatureResponse>))]
        public Task<IActionResult> GetProjectTrackingSubFeature(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-project-tracking-sub-feature")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getProjectTrackingSubFeatureHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region SPC List
        [FunctionName(nameof(ProjectInformationEndpoint.GetSpcList))]
        [OpenApiOperation(tags: _tag, Summary = "Get SPC List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSPCListRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetSPCListResponse>))]
        public Task<IActionResult> GetSpcList(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-spc-list")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getSPCListHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveSPC))]
        [OpenApiOperation(tags: _tag, Summary = "Save SPC")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveSPCRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveSPC([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-spc")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveSPCHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ProjectInformationEndpoint.DeleteSpc))]
        [OpenApiOperation(tags: _tag, Summary = "Delete SPC")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteSPCRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteSpc([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/delete-spc")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteSPCHandler.Execute(req, cancellationToken);
        }

        #endregion
    }
}
