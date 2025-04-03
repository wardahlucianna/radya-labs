using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementPortal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnGuidanceCounseling.UnivInformationManagementPortal
{
    public class UnivInformationManagementPortalEndPoint
    {
        private const string _route = "guidance-counseling/university-information-management-portal";
        private const string _tag = "University Information Management Portal";

        #region Portal
        [FunctionName(nameof(UnivInformationManagementPortalEndPoint.GetListUnivInformationManagementPortal))]
        [OpenApiOperation(tags: _tag, Summary = "Get University Information Management Portal")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUnivInformationManagementPortalRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUnivInformationManagementPortalRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnivInformationManagementPortalRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnivInformationManagementPortalRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUnivInformationManagementPortalRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUnivInformationManagementPortalRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnivInformationManagementPortalRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnivInformationManagementPortalRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUnivInformationManagementPortalResult))]
        public Task<IActionResult> GetListUnivInformationManagementPortal(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UnivInformationManagementPortalHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(UnivInformationManagementPortalEndPoint.AddUnivInformationManagementPortal))]
        [OpenApiOperation(tags: _tag, Summary = "Add University Information Management Portal")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddUnivInformationManagementPortalRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddUnivInformationManagementPortal(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UnivInformationManagementPortalHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UnivInformationManagementPortalEndPoint.UpdateUnivInformationManagementPortal))]
        [OpenApiOperation(tags: _tag, Summary = "Update University Information Management Portal")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateUnivInformationManagementPortalRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateUnivInformationManagementPortal(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UnivInformationManagementPortalHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UnivInformationManagementPortalEndPoint.GetUnivInformationManagementPortalDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get University Information Management Portal Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUnivInformationManagementPortalResult))]
        public Task<IActionResult> GetUnivInformationManagementPortalDetail(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/{id}")] HttpRequest req,
        string id,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UnivInformationManagementPortalHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }

        [FunctionName(nameof(UnivInformationManagementPortalEndPoint.DeleteUnivInformationManagementPortal))]
        [OpenApiOperation(tags: _tag, Summary = "Delete University Information Management Portal")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteUnivInformationManagementPortal(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UnivInformationManagementPortalHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region portal-approval

        [FunctionName(nameof(UnivInformationManagementPortalEndPoint.GetListUnivInformationManagementPortalApproval))]
        [OpenApiOperation(tags: _tag, Summary = "Get University Information Management Portal Approval")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUnivInformationManagementPortalApprovalRequest.IdSchool), In = ParameterLocation.Query,Required = true)]
        [OpenApiParameter(nameof(GetUnivInformationManagementPortalApprovalRequest.IdFromSchool), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnivInformationManagementPortalApprovalRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnivInformationManagementPortalApprovalRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnivInformationManagementPortalApprovalRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUnivInformationManagementPortalApprovalRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUnivInformationManagementPortalApprovalRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnivInformationManagementPortalApprovalRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnivInformationManagementPortalApprovalRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUnivInformationManagementPortalApprovalResult))]
        public Task<IActionResult> GetListUnivInformationManagementPortalApproval(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-approval")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetUnivInformationManagementPortalApprovalHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(UnivInformationManagementPortalEndPoint.AddUnivInformationManagementPortalApproval))]
        [OpenApiOperation(tags: _tag, Summary = "Process University Information Management Portal Approval")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddUnivInformationManagementPortalApprovalRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddUnivInformationManagementPortalApproval(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-process-approval")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UnivInformationManagementPortalProcessApprovalHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

    }
}
