using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.User.FnBlocking.Blocking;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.User.FnBlocking.Blocking
{
    public class BlockingEndPoint
    {
        private const string _route = "user-blocking/blocking";
        private const string _tag = "Blocking";

        [FunctionName(nameof(BlockingEndPoint.GetBlocking))]
        [OpenApiOperation(tags: _tag, Summary = "Get Blocking")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetBlockingRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetBlockingRequest.IdFeature), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetBlockingRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetBlockingResult))]
        public Task<IActionResult> GetBlocking(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetBlockingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BlockingEndPoint.GetBlockingStatusMobile))]
        [OpenApiOperation(tags: _tag, Summary = "Get Blocking Status Mobile")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetBlockingStatusMobileRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetBlockingStatusMobileRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetBlockingStatusMobileResult))]
        public Task<IActionResult> GetBlockingStatusMobile(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-status-mobile")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetBlockingStatusMobileHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BlockingEndPoint.GetListBlocking))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Blocking")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListBlockingRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListBlockingRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListBlockingResult))]
        public Task<IActionResult> GetListBlocking(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-list")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListStudentBlockingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BlockingEndPoint.GetListBlockingDashboard))]
        [OpenApiOperation(tags: _tag, Summary = "Get Blocking Dashboard")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetBlockingDashboardRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetBlockingDashboardRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetBlockingDashboardResult))]
        public Task<IActionResult> GetListBlockingDashboard(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-dashboard")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetBlockingDashboardHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
