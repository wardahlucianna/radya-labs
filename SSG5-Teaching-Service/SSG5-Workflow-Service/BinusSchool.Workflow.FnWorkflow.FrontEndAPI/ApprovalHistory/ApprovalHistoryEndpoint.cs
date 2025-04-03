using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Workflow.FnWorkflow.ApprovalHistory
{
    public class ApprovalHistoryEndpoint
    {
        private const string _route = "workflow/approval-history";
        private const string _tag = "Workflow Approval History";

        private readonly ApprovalHistoryHandler _approvalHistoryHandler;
        private readonly GetApprovalHistoryByUserHanlder _getApprovalHistoryByUserHanlder;
        private readonly UpdateTaskHistoryhandler _updateTaskHistoryhandler;
        public ApprovalHistoryEndpoint(ApprovalHistoryHandler approvalHistoryHandler,
            GetApprovalHistoryByUserHanlder getApprovalHistoryByUserHanlder,
            UpdateTaskHistoryhandler updateTaskHistoryhandler)
        {
            _approvalHistoryHandler = approvalHistoryHandler;
            _getApprovalHistoryByUserHanlder = getApprovalHistoryByUserHanlder;
            _updateTaskHistoryhandler = updateTaskHistoryhandler;
        }

        [FunctionName(nameof(ApprovalHistoryEndpoint.GetApprovalHistoryByuser))]
        [OpenApiOperation(tags: _tag, Summary = "get Approval history by user")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetApprovalHistoryByUserRequest.IdUserAction), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetApprovalHistoryByUserRequest.IdDocument), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetApprovalHistoryByUserRequest.Action), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetApprovalHistoryByUserResult))]
        public Task<IActionResult> GetApprovalHistoryByuser(
             [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route+"-by-user")] HttpRequest req,
             CancellationToken cancellationToken)
        {
            return _getApprovalHistoryByUserHanlder.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ApprovalHistoryEndpoint.AddApprovalHistory))]
        [OpenApiOperation(tags: _tag, Summary = "Add Approval history")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddApprovalHistoryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddApprovalHistory(
             [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
             CancellationToken cancellationToken)
        {
            return _approvalHistoryHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(ApprovalHistoryEndpoint.UpdateApprovalHistory))]
        [OpenApiOperation(tags: _tag, Summary = "Update Approval history")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateApprovalHistoryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateApprovalHistory(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _approvalHistoryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ApprovalHistoryEndpoint.UpdateTaskApprovalHistory))]
        [OpenApiOperation(tags: _tag, Summary = "Update task Approval history")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateTaskHistoryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateTaskApprovalHistory(
         [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route+"-update-task")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _updateTaskHistoryhandler.Execute(req, cancellationToken);
        }
    }
}
