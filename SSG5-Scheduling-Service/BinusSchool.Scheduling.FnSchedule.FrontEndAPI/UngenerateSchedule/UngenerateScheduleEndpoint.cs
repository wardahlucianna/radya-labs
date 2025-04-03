using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.UngenerateSchedule
{
    public class UngenerateScheduleEndpoint
    {
        private const string _route = "schedule/ungenerate-schedule";
        private const string _tag = "Ungenerate Schedule";

        [FunctionName(nameof(UngenerateScheduleEndpoint.GetScheduleGrades))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade In Current Schedule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetScheduleGradesRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetScheduleGradesRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetScheduleGradesRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetScheduleGradesRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetScheduleGradesRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetScheduleGradesRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetScheduleGradesRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetScheduleGradesRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetScheduleGradesRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetScheduleGradesRequest.IdAscTimetable), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetScheduleGradesRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetScheduleGradesRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGradeResult))]
        public Task<IActionResult> GetScheduleGrades(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-grade")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetScheduleGradesHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UngenerateScheduleEndpoint.GetScheduleLessonsByGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Get Lesson by Grade In Current Schedule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetScheduleLessonsByGradeRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetScheduleLessonsByGradeRequest.IdAscTimetable), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetScheduleLessonsByGradeRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetScheduleLessonsByGradeRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<ItemValueVm>))]
        public Task<IActionResult> GetScheduleLessonsByGrade(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-lesson-by-grade")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetScheduleLessonsByGradeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UngenerateScheduleEndpoint.GetScheduleLessonsByGradeV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Lesson by Grade In Current Schedule V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetScheduleLessonsByGradeRequest), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<GetScheduleLessonsByGradeV2Result>))]
        public Task<IActionResult> GetScheduleLessonsByGradeV2(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-lesson-by-grade-v2")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetScheduleLessonsByGradeV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UngenerateScheduleEndpoint.GetScheduleLessonsByStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Lesson by Student In Current Schedule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetScheduleLessonsByStudentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetScheduleLessonsByStudentRequest.IdAscTimetable), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetScheduleLessonsByStudentRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetScheduleLessonsByStudentRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<ItemValueVm>))]
        public Task<IActionResult> GetScheduleLessonsByStudent(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-lesson-by-student")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetScheduleLessonsByStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UngenerateScheduleEndpoint.DeleteUngenerateScheduleGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Ungenerate Schedule per Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteUngenerateScheduleGradeRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteUngenerateScheduleGrade(
           [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/grade")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteUngenerateScheduleGradeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UngenerateScheduleEndpoint.DeleteUngenerateScheduleGradeV2))]
        [OpenApiOperation(tags: _tag, Summary = "Ungenerate Schedule per Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteUngenerateScheduleGradeV2Request), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteUngenerateScheduleGradeV2(
           [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/grade-v2")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteUngenerateScheduleGradeV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UngenerateScheduleEndpoint.DeleteUngenerateScheduleStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Ungenerate Schedule per Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteUngenerateScheduleStudentRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteUngenerateScheduleStudent(
           [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/student")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteUngenerateScheduleStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UngenerateScheduleEndpoint.DeleteUngenerateScheduleStudentHomeroom))]
        [OpenApiOperation(tags: _tag, Summary = "Ungenerate Schedule per Student per homeroom")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteUngenerateScheduleStudentHomeroomRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteUngenerateScheduleStudentHomeroom(
           [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/student/homeroom")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteUngenerateScheduleStudentHomeroomHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UngenerateScheduleEndpoint.UpdateGenerateScheduleStudentHomeroom))]
        [OpenApiOperation(tags: _tag, Summary = "Update Generate Schedule per Student per homeroom")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateGenerateScheduleStudentHomeroomRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateGenerateScheduleStudentHomeroom(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = "schedule/update-generate-schedule/student/homeroom")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateGenerateScheduleStudentHomeroomHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UngenerateScheduleEndpoint.GetDayByGarde))]
        [OpenApiOperation(tags: _tag, Summary = "Get Day by grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDayByGardeRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDayByGardeRequest.IdAscTimetable), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDayByGardeRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDayByGardeRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<ItemValueVm>))]
        public Task<IActionResult> GetDayByGarde(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-day-by-grade")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDayByGardeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UngenerateScheduleEndpoint.GetAscNameUngenerateSchedule))]
        [OpenApiOperation(tags: _tag, Summary = "Get Asc Name Ungenerate Schedule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAscNameUngenerateScheduleRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<ItemValueVm>))]
        public Task<IActionResult> GetAscNameUngenerateSchedule(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-asc-name")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAscNameUngenerateScheduleHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UngenerateScheduleEndpoint.GetGradeUngenerateSchedule))]
        [OpenApiOperation(tags: _tag, Summary = "Get Asc Name Ungenerate Schedule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGradeUngenerateScheduleRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGradeUngenerateScheduleRequest.IdAscTimetable), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<ItemValueVm>))]
        public Task<IActionResult> GetGradeUngenerateSchedule(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-grade")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGradeUngenerateScheduleHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
