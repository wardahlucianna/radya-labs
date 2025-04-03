using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Attendance.FnAttendance.ClassSession;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Attendance.FnAttendance.ClassSession
{
    public class ClassSessionEndpoint
    {
        private const string _route = "class-and-session";
        private const string _tag = "Schedule Class And Session";

        [FunctionName(nameof(ClassSessionEndpoint.GetClassAndSessions))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetClassSessionRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassSessionRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassSessionRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassSessionRequest.Date), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClassSessionResult[]))]
        public Task<IActionResult> GetClassAndSessions(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetClassSessionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassSessionEndpoint.GetClassAndSessionsV2))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetClassSessionRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassSessionRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassSessionRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassSessionRequest.Date), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClassSessionResult[]))]
        public Task<IActionResult> GetClassAndSessionsV2(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route+"-V2")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetClassSessionV2Handler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
