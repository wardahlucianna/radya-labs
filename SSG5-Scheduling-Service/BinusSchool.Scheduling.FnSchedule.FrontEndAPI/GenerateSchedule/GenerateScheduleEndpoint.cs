using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.GenerateSchedule
{
    public class GenerateScheduleEndpoint
    {
        private const string _route = "schedule/generate-schedule";
        private const string _tag = "Generate schedule";

        [FunctionName(nameof(GenerateScheduleEndpoint.GetgenerateScheduleWithClassID))]
        [OpenApiOperation(tags: _tag, Summary = "Get Generate schedule with class id for edit schedule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGenerateScheduleWithClassIDRequest.IdAscTimetable), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGenerateScheduleWithClassIDRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGenerateScheduleWithClassIDRequest.ClassID), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClassIDByGradeAndStudentResult))]
        public Task<IActionResult> GetgenerateScheduleWithClassID(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-generate-schedule-with-classid")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGenerateScheduleWithClassIDHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GenerateScheduleEndpoint.GetClassIDWithSudentGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Get Class Id with Student Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetClassIDByGradeAndStudentRequest.IdAscTimetable), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassIDByGradeAndStudentRequest.IdGrade), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetClassIDByGradeAndStudentRequest.IdStudent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassIDByGradeAndStudentRequest.ClassID), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClassIDByGradeAndStudentResult))]
        public Task<IActionResult> GetClassIDWithSudentGrade(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-classids-grade-student")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetClassIDWithGradeAndStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GenerateScheduleEndpoint.GetDetailByMountYears))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Oncalender by mount and years")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.IdAcademicYears), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.IdAscTimetable), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.IdClass), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.DateMountYers), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailGenerateScheduleResult))]
        public Task<IActionResult> GetDetailByMountYears(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-by-mount-year")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailGenerateScheduleHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GenerateScheduleEndpoint.GetDetailByMonthYearsV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Oncalender by mount and years V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.IdAcademicYears), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.IdAscTimetable), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.IdClass), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.DateMountYers), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailGenerateScheduleResult))]
        public Task<IActionResult> GetDetailByMonthYearsV2(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-by-month-year-v2")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailGenerateScheduleV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GenerateScheduleEndpoint.GetDetailByDate))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail On calender by Date")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.IdAcademicYears), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.IdAscTimetable), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.IdClass), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.ScheduleDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailGenerateScheduleResult))]
        public Task<IActionResult> GetDetailByDate(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-by-date")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailLoadMoreHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GenerateScheduleEndpoint.GetDetailByDateV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail On calender by Date V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.IdAcademicYears), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.IdAscTimetable), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.IdClass), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailGenerateScheduleRequest.ScheduleDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailGenerateScheduleResult))]
        public Task<IActionResult> GetDetailByDateV2(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-by-date-v2")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailLoadMoreV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GenerateScheduleEndpoint.GetGenerateScheduleGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Get Generate Schedule Detail with Grade View")]
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
        [OpenApiParameter(nameof(GetGenerateScheduleGradeRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeRequest.IdAscTimetable), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGenerateScheduleGradeResult[]))]
        public Task<IActionResult> GetGenerateScheduleGrade(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/grade")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGenerateScheduleGradeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GenerateScheduleEndpoint.GetGenerateScheduleGradeV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Generate Schedule Detail with Grade View V2")]
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
        [OpenApiParameter(nameof(GetGenerateScheduleGradeRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeRequest.IdAscTimetable), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGenerateScheduleGradeResult[]))]
        public Task<IActionResult> GetGenerateScheduleGradeV2(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/grade-v2")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGenerateScheduleGradeV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GenerateScheduleEndpoint.GetGenerateScheduleStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Generate Schedule Detail with Student View")]
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
        [OpenApiParameter(nameof(GetGenerateScheduleStudentRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGenerateScheduleStudentRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGenerateScheduleStudentRequest.Semester), In = ParameterLocation.Query, Type = typeof(int?))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGeneratedScheduleStudentResult[]))]
        public Task<IActionResult> GetGenerateScheduleStudent(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGenerateScheduleStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GenerateScheduleEndpoint.GetGenerateScheduleGradeHistory))]
        [OpenApiOperation(tags: _tag, Summary = "Get Generate Schedule Grade History")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.IdAscTimetable), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.StartPeriod), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.EndPeriod), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.ClassId), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGenerateScheduleGradeResult[]))]
        public Task<IActionResult> GetGenerateScheduleGradeHistory(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/grade-history")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGenerateScheduleGradeHistoryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GenerateScheduleEndpoint.GetGenerateScheduleGradeHistoryV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Generate Schedule Grade History")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.IdAscTimetable), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.StartPeriod), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.EndPeriod), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGenerateScheduleGradeHistoryRequest.ClassId), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGenerateScheduleGradeResult[]))]
        public Task<IActionResult> GetGenerateScheduleGradeHistoryV2(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/grade-history-v2")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGenerateScheduleGradeHistoryV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GenerateScheduleEndpoint.CheckIsJobRunning))]
        [OpenApiOperation(tags: _tag, Summary = "Check is there are running job on generate schedule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(StartGeneratedScheduleProcessRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(StartGeneratedScheduleProcessRequest.Grades), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(bool))]
        public Task<IActionResult> CheckIsJobRunning(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/check/is-there-job-run")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CheckGeneratedScheduleProcessHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GenerateScheduleEndpoint.StartProcess))]
        [OpenApiOperation(tags: _tag, Summary = "Start generate schedule process")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(StartGeneratedScheduleProcessRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> StartProcess(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/start-process")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<StartGeneratedScheduleProcessHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GenerateScheduleEndpoint.FinishProcess))]
        [OpenApiOperation(tags: _tag, Summary = "Finish generate schedule process")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(FinishGeneratedScheduleProcessRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> FinishProcess(
          [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/finish-process")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<FinishGeneratedScheduleProcessHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GenerateScheduleEndpoint.GetGradesHasGeneratedSchedule))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade Has Generated Schedule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGradeRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetGradeRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGradeRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGradeRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetGradeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGradeRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGradeResult))]
        public Task<IActionResult> GetGradesHasGeneratedSchedule(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/grade-has-generated-schedule")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGradeHasGeneratedScheduleHandler>();
            return handler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(GenerateScheduleEndpoint.GetGradesHasGeneratedScheduleV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade Has Generated Schedule V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGradeRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetGradeRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGradeRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGradeRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetGradeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGradeRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGradeResult))]
        public Task<IActionResult> GetGradesHasGeneratedScheduleV2(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/grade-has-generated-schedule-v2")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGradeHasGeneratedScheduleHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GenerateScheduleEndpoint.GetGradeByAscTimetable))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade By Asc Timetable")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGradeByAscTimetableRequest.IdAscTimetable), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGradeByAscTimetableResult))]
        public Task<IActionResult> GetGradeByAscTimetable(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-grade-by-asc-timetable")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGradeByAscTimetableHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GenerateScheduleEndpoint.GetWeekByGradeSubject))]
        [OpenApiOperation(tags: _tag, Summary = "Get Week by Grade Subject")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetWeekByGradeSubjectRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetWeekByGradeSubjectResult))]
        public Task<IActionResult> GetWeekByGradeSubject(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-week-by-grade-subject")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetWeekByGradeSubjectHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
