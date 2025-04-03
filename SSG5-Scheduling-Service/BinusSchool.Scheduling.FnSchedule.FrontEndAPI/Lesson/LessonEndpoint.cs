using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.Lesson
{
    public class LessonEndpoint
    {
        private const string _route = "schedule/lesson";
        private const string _tag = "Lesson";

        [FunctionName(nameof(LessonEndpoint.GetTeacherByLesson))]
        [OpenApiOperation(tags: _tag, Summary = "Get Teacher By Lesson")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTeacherByLessonRequest.IdLesson), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DataModelGeneral[]))]
        public Task<IActionResult> GetTeacherByLesson(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get/teacher")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTeacherByLessonHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonEndpoint.GetWeekByLesson))]
        [OpenApiOperation(tags: _tag, Summary = "Get Week By Lesson")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetWeekByLessonRequest.IdLesson), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DataModelGeneral[]))]
        public Task<IActionResult> GetWeekByLesson(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get/week")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetWeekByLessonHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonEndpoint.GetDownloadLesson))]
        [OpenApiOperation(tags: _tag, Summary = "Get Download Lesson List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLessonRequest.ExceptIds), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetLessonRequest.IdAcadyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLessonRequest.Semester), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetLessonRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonRequest.IdHomeroom), In = ParameterLocation.Query, Description = "GET /scheduling-fn-schedule/schedule/homeroom")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLessonResult[]))]
        public Task<IActionResult> GetDownloadLesson(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/download")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDownloadLessonHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonEndpoint.GetLessons))]
        [OpenApiOperation(tags: _tag, Summary = "Get Lesson List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetLessonRequest.ExceptIds), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetLessonRequest.IdAcadyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLessonRequest.Semester), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetLessonRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonRequest.IdHomeroom), In = ParameterLocation.Query, Description = "GET /scheduling-fn-schedule/schedule/homeroom")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLessonResult[]))]
        public Task<IActionResult> GetLessons(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetLessonHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonEndpoint.GetLessonDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Lesson Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLessonDetailResult))]
        public Task<IActionResult> GetLessonDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetLessonDetailHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }

        [FunctionName(nameof(LessonEndpoint.AddLesson))]
        [OpenApiOperation(tags: _tag, Summary = "Add Lesson", Description = @"
            - IdHomeroom: GET /scheduling-fn-scedule/schedule/homeroom | field: payload.id
            - IdPathway: GET /scheduling-fn-schedule/schedule/homeroom | field: payload.pathways.id")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddLessonRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddLesson(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddLessonHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonEndpoint.UpdateLesson))]
        [OpenApiOperation(tags: _tag, Summary = "Update Lesson", Description = @"
            - IdPathway: GET /scheduling-fn-schedule/schedule/homeroom | field: payload.pathways.id")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateLessonRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateLesson(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateLessonHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonEndpoint.DeleteLesson))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Lesson")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(string[]), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteLesson(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteLessonHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonEndpoint.GetLessonByTeacherID))]
        [OpenApiOperation(tags: _tag, Summary = "Get Lesson By Teacher ID")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetLessonByTeacherIDRequest.IdTeacher), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLessonByTeacherIDResult[]))]
        public Task<IActionResult> GetLessonByTeacherID(
             [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-byteacherid")] HttpRequest req,
             CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetLessonByTeacherIDHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonEndpoint.ValidateGeneratedClassId))]
        [OpenApiOperation(tags: _tag, Summary = "Validate generated class id")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(ValidateGeneratedClassIdRequest.IdAcadyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(ValidateGeneratedClassIdRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(ValidateGeneratedClassIdRequest.Semester), In = ParameterLocation.Query, Required = true, Type = typeof(int))]
        [OpenApiParameter(nameof(ValidateGeneratedClassIdRequest.IdSubject), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(ValidateGeneratedClassIdRequest.ClassIdToValidate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(ValidateGeneratedClassIdRequest.BookedClassIds), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ValidateGeneratedClassIdResult))]
        public Task<IActionResult> ValidateGeneratedClassId(
             [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-validate-classid")] HttpRequest req,
             CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ValidateGeneratedClassIdHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonEndpoint.AddLessonCopy))]
        [OpenApiOperation(tags: _tag, Summary = "Add Lesson Copy")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddLessonCopyRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddLessonCopy(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-copy")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddLessonCopyHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
