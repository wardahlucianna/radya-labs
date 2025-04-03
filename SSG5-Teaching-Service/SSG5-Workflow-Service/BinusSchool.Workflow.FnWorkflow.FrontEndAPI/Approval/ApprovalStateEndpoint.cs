using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval.ApprovalByEmail;
using BinusSchool.Workflow.FnWorkflow.Approval.ApprovalByEmail;
using BinusSchool.Workflow.FnWorkflow.SendEmail.ApprovalByEmail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Workflow.FnWorkflow.Approval
{
    public class ApprovalStateEndpoint
    {
        private const string _route = "workflow/approval-state";
        private const string _tag = "Workflow Approval";

        private readonly GetApprovalStateHandler _getapprovalStateHandler;
        private readonly GetInApproveHandler _getInApproveHandler;
        private readonly GetListApprovalStateWithWorkflowHandler _getListApprovalStateWithWorkflowHandler;
        private readonly CreateApprovalTokenHandler _createApprovalTokenHandler;
        private readonly ValidateApprovalTokenHandler _validateApprovalTokenHandler;
        private readonly DeactivateApprovalTokenHandler _deactivateApprovalTokenHandler;

        public ApprovalStateEndpoint(GetApprovalStateHandler getapprovalStateHandler,
            GetInApproveHandler getInApproveHandler,
            GetListApprovalStateWithWorkflowHandler getListApprovalStateWithWorkflowHandler,
            CreateApprovalTokenHandler createApprovalTokenHandler,
            ValidateApprovalTokenHandler validateApprovalTokenHandler,
            DeactivateApprovalTokenHandler deactivateApprovalTokenHandler)
        {
            _getapprovalStateHandler = getapprovalStateHandler;
            _getInApproveHandler = getInApproveHandler;
            _getListApprovalStateWithWorkflowHandler = getListApprovalStateWithWorkflowHandler;
            _createApprovalTokenHandler = createApprovalTokenHandler;
            _validateApprovalTokenHandler = validateApprovalTokenHandler;
            _deactivateApprovalTokenHandler = deactivateApprovalTokenHandler;
        }

        [FunctionName(nameof(ApprovalStateEndpoint.GetApprovalStateByWorkflow))]
        [OpenApiOperation(tags: _tag, Summary = "Get approval state by Workflow")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetApprovalStateByWorkflowRequest.IdApprovalWorkflow), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetApprovalStateByWorkflowRequest.StateType), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetApprovalStateByWorkflowResult))]
        public Task<IActionResult> GetApprovalStateByWorkflow(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-by-workflow")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _getapprovalStateHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ApprovalStateEndpoint.GetListApprovalStateByWorkflow))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListApprovalStateWithWorkflowRequest.IdWorkflow), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListApprovalStateWithWorkflowRequest.WithoutState), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetApprovalStateByWorkflowResult))]
        public Task<IActionResult> GetListApprovalStateByWorkflow(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-by-workflow-without")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _getListApprovalStateWithWorkflowHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ApprovalStateEndpoint.GetInApprove))]
        [OpenApiOperation(tags: _tag, Summary = "Get In Approve")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetInApproveRequest.IdFromState), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetInApproveRequest.Action), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetInApproveResult))]
        public Task<IActionResult> GetInApprove(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-in-approve")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _getInApproveHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ApprovalStateEndpoint.CreateApprovalToken))]
        [OpenApiOperation(tags: _tag, Summary = "Create Approval Token (Approval By Email)")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CreateApprovalTokenRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CreateApprovalTokenResult))]
        public Task<IActionResult> CreateApprovalToken(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-create-approval-token")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _createApprovalTokenHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ApprovalStateEndpoint.ValidateApprovalToken))]
        [OpenApiOperation(tags: _tag, Summary = "Validate Approval Token From Email")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ValidateApprovalTokenRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ValidateApprovalToken(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-validate-approval-token")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _validateApprovalTokenHandler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(ApprovalStateEndpoint.DeactivateApprovalToken))]
        [OpenApiOperation(tags: _tag, Summary = "Deactivate Approval Token")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeactivateApprovalTokenRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeactivateApprovalToken(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-deactivate-approval-token")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _deactivateApprovalTokenHandler.Execute(req, cancellationToken, false);
        }
    }
}
