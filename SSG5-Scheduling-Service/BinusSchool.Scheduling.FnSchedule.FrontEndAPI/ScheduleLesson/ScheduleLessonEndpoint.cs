using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleLesson;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleLesson
{
    public class ScheduleLessonEndpoint
    {
        private const string _route = "schedule/schedule-lesson";
        private const string _tag = "Schedule Lesson";

        private readonly ScheduleLessonHandler _scheduleLessonHandler;
        public ScheduleLessonEndpoint(ScheduleLessonHandler scheduleLessonHandler)
        {
            _scheduleLessonHandler = scheduleLessonHandler;
        }

        [FunctionName(nameof(ScheduleLessonEndpoint.GetScheduleLesson))]
        [OpenApiOperation(tags: _tag, Summary = "Get Event Parent and Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(ScheduleLessonRequest.idAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> GetScheduleLesson(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ScheduleLessonHandler>();
            return handler.Execute(req, cancellationToken);
        }

    }
}
