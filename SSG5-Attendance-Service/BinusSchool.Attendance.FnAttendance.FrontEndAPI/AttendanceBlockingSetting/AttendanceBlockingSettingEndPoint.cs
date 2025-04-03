using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceBlockingSetting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Attendance.FnAttendance.AttendanceBlockingSetting
{
    public class AttendanceBlockingSettingEndPoint
    {
        private const string _route = "attendance-blocking-setting";
        private const string _tag = "Attendance Blocking Setting";

        [FunctionName(nameof(AttendanceBlockingSettingEndPoint.GetBlockingMapAttendanceAY))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetBlockingMapAttendanceAYRequest.IdAcadyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetBlockingMapAttendanceAYResult[]))]
        public Task<IActionResult> GetBlockingMapAttendanceAY(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/mapping")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetBlockingMapAttendanceAYHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceBlockingSettingEndPoint.GetAttendanceBlockingSetting))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceBlockingSettingDetailRequest.IdAcadyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceBlockingSettingDetailRequest.IdBlockingType), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceBlockingSettingDetailResult[]))]
        public Task<IActionResult> GetAttendanceBlockingSetting(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceBlockingSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceBlockingSettingEndPoint.UpdateAttendanceBlockingSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Update Blocking Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateAttendanceBlockingSettingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateAttendanceBlockingSetting(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateAttendanceBlockingSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceBlockingSettingEndPoint.GetAttendanceBlockingSettingMessage))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceBlockingSettingMessageRequest.IdAcadyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceBlockingSettingMessageResult[]))]
        public Task<IActionResult> GetAttendanceBlockingSettingMessage(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/message")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceBlockingSettingMessageHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
