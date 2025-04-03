using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.BreakSetting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.BreakSetting
{
    public class BreakSettingEndpoint
    {
        private const string _route = "schedule/break-setting";
        private const string _tag = "Break Setting";

        public BreakSettingEndpoint()
        {
        }

        [FunctionName(nameof(BreakSettingEndpoint.GetBreakSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Get Break Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetBreakSettingRequest.IdUserTeacher), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetBreakSettingRequest.DateCalender), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(GetBreakSettingRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetBreakSettingRequest.IdInvitationBookingSetting), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBreakSettingRequest.DateInvitation), In = ParameterLocation.Query, Type = typeof(DateTime))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetBreakSettingResult))]
        public Task<IActionResult> GetBreakSetting(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetBreakSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BreakSettingEndpoint.GetBreakSettingV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Break Setting V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetBreakSettingRequest.IdUserTeacher), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetBreakSettingRequest.DateCalender), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(GetBreakSettingRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetBreakSettingRequest.IdInvitationBookingSetting), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBreakSettingRequest.DateInvitation), In = ParameterLocation.Query, Type = typeof(DateTime))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetBreakSettingResult))]
        public Task<IActionResult> GetBreakSettingV2(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-v2")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetBreakSettingV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BreakSettingEndpoint.UpdatePriorityAndFlexibleBreak))]
        [OpenApiOperation(tags: _tag, Summary = "Update Priority And Flexible Break")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdatePriorityAndFlexibleBreakRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdatePriorityAndFlexibleBreak(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-priority-flexible-break")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdatePriorityAndFlexibleBreakHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BreakSettingEndpoint.UpdateAvailability))]
        [OpenApiOperation(tags: _tag, Summary = "Update Availability")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateAvailabilityRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateAvailability(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-availability")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateAvailabilityHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
