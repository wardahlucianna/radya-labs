using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Util.FnConverter.CalendarEventToPdf;
using BinusSchool.Data.Model.Util.FnConverter.CreativityActivityService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Util.FnConverter.CreativityActivityServiceToPdf
{
    public class CreativityActivityServiceToPdfEndpoint
    {
        private const string _route = "creativity-activity-service-to-pdf";
        private const string _tag = "Creativity Activity Service to Pdf";

        [FunctionName(nameof(CreativityActivityServiceToPdfEndpoint.TimelineToPdf))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(TimelineToPdfRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(TimelineToPdfRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(TimelineToPdfRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(TimelineToPdfRequest.Role), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(TimelineToPdfRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true, Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ConvertExperienceToPdfResult))]
        public Task<IActionResult> TimelineToPdf(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route+"/timeline")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<TimelineToPdfHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(CreativityActivityServiceToPdfEndpoint.ExperienceToPdf))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(ExperienceToPdfRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(ExperienceToPdfRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(ExperienceToPdfRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(ExperienceToPdfRequest.Role), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(ExperienceToPdfRequest.IsComment), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(ExperienceToPdfRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true, Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ConvertExperienceToPdfResult[]))]
        public Task<IActionResult> ExperienceToPdf(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/experience")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ExperienceToPdfHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
    }
}
