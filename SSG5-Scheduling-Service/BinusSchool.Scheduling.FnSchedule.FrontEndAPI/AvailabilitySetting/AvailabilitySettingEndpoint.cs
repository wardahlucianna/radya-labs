using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AvailabilitySetting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.AvailabilitySetting
{
    public class AvailabilitySettingEndpoint
    {
        private const string _route = "schedule/availability-setting";
        private const string _tag = "Availability Setting";

        private readonly AvailabilitySettingHandler _availabilitySettingHandler;
        public AvailabilitySettingEndpoint(AvailabilitySettingHandler AvailabilitySettingHandler)
        {
            _availabilitySettingHandler = AvailabilitySettingHandler;
        }

        [FunctionName(nameof(AvailabilitySettingEndpoint.GetAvailabilitySetting))]
        [OpenApiOperation(tags: _tag, Summary = "Get Available Time")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAvailabilitySettingRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAvailabilitySettingRequest.IdUserTeacher), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAvailabilitySettingRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAvailabilitySettingResult))]
        public Task<IActionResult> GetAvailabilitySetting(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _availabilitySettingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AvailabilitySettingEndpoint.DetailAvailabilitySetting))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Available Time")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DetailAvailabilitySettingRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DetailAvailabilitySettingRequest.IdUserTeacher), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DetailAvailabilitySettingRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DetailAvailabilitySettingRequest.Day), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailAvailabilitySettingResult))]
        public Task<IActionResult> DetailAvailabilitySetting(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route+"/Detail")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DetailAvailabilitySettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AvailabilitySettingEndpoint.AddAvailabilitySetting))]
        [OpenApiOperation(tags: _tag, Summary = "Create Available Time")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddAvailabilitySettingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddAvailabilitySetting(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _availabilitySettingHandler.Execute(req, cancellationToken);
        }
    }
}
