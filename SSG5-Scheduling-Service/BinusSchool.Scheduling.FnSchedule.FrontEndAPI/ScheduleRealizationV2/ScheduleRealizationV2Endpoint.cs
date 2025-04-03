using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using BinusSchool.Scheduling.FnSchedule.ScheduleRealization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class ScheduleRealizationV2Endpoint
    {
        private const string _route = "schedule/schedule-realizationv2";
        private const string _tag = "Schedule Realization V2";

        [FunctionName(nameof(ScheduleRealizationV2Endpoint.GetSessionVenueForScheduleRealization))]
        [OpenApiOperation(tags: _tag, Summary = "Get Session or Venue by Date AY,Teacher,Level,Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSessionVenueForScheduleRealizationRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSessionVenueForScheduleRealizationRequest.IdUser), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetSessionVenueForScheduleRealizationRequest.IdLevel), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetSessionVenueForScheduleRealizationRequest.IdGrade), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetSessionVenueForScheduleRealizationRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSessionVenueForScheduleRealizationRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSessionVenueForScheduleRealizationRequest.IsSession), In = ParameterLocation.Query, Type = typeof(bool), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSessionVenueForScheduleRealizationResult))]
        public Task<IActionResult> GetSessionVenueForScheduleRealization(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-session-venue-for-schedule-realization")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSessionVenueForScheduleRealizationHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationV2Endpoint.GetListScheduleRealizationV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Schedule Realization V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListScheduleRealizationV2Request.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListScheduleRealizationV2Request.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListScheduleRealizationV2Request.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListScheduleRealizationV2Request.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListScheduleRealizationV2Request.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListScheduleRealizationV2Request.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListScheduleRealizationV2Request.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListScheduleRealizationV2Request.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListScheduleRealizationV2Request.IdGrade), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetListScheduleRealizationV2Request.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListScheduleRealizationV2Request.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListScheduleRealizationV2Request.IdUserTeacher), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetListScheduleRealizationV2Request.SessionID), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListScheduleRealizationV2Request.IdVenue), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListScheduleRealizationV2Result))]
        public Task<IActionResult> GetListScheduleRealizationV2(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-schedule-realization")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListScheduleRealizationV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationV2Endpoint.SaveScheduleRealizationV2))]
        [OpenApiOperation(tags: _tag, Summary = "Save Schedule Realization V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveScheduleRealizationV2Request))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> SaveScheduleRealizationV2(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-schedule-realization")] HttpRequest req,
            [Queue("notification-sr")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SaveScheduleRealizationV2Handler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(ScheduleRealizationV2Endpoint.UndoScheduleRealizationV2))]
        [OpenApiOperation(tags: _tag, Summary = "Undo Schedule Realization V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UndoScheduleRealizationV2Request))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UndoScheduleRealizationV2(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/undo-schedule-realization")] HttpRequest req,
            [Queue("notification-sr")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UndoScheduleRealizationV2Handler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(ScheduleRealizationV2Endpoint.ResetScheduleRealizationV2))]
        [OpenApiOperation(tags: _tag, Summary = "Undo Schedule Realization V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UndoScheduleRealizationV2Request))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> ResetScheduleRealizationV2(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/reset-schedule-realization")] HttpRequest req,
            [Queue("notification-sr")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ResetScheduleRealizationHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(ScheduleRealizationV2Endpoint.GetClassIdByTeacherDateV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Class ID by Teacher Date AY V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetClassIdByTeacherDateV2Request.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassIdByTeacherDateV2Request.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassIdByTeacherDateV2Request.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassIdByTeacherDateV2Request.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClassIdByTeacherDateV2Result))]
        public Task<IActionResult> GetClassIdByTeacherDateV2(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-class-id-by-teacher-date")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetClassIdBYTeacherDateV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationV2Endpoint.GetListScheduleRealizationByTeacherv2))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Schedule Realization By Teacher V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherV2Request.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherV2Request.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherV2Request.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherV2Request.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherV2Request.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherV2Request.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherV2Request.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherV2Request.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherV2Request.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherV2Request.IdUserTeacher), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherV2Request.ClassID), In = ParameterLocation.Query, Type = typeof(string[]), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListScheduleRealizationByTeacherV2Result))]
        public Task<IActionResult> GetListScheduleRealizationByTeacherv2(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-schedule-realization-by-teacher")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListScheduleRealizationByTeacherV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationV2Endpoint.SaveScheduleRealizationByTeacherV2))]
        [OpenApiOperation(tags: _tag, Summary = "Save Schedule Realization by Teacher V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveScheduleRealizationByTeacherV2Request))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> SaveScheduleRealizationByTeacherV2(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-schedule-realization-by-teacher")] HttpRequest req,
            [Queue("notification-sr")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SaveScheduleRealizationByTeacherV2Handler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(ScheduleRealizationV2Endpoint.UndoScheduleRealizationByTeacherV2))]
        [OpenApiOperation(tags: _tag, Summary = "Undo Schedule Realization By Teacher V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UndoScheduleRealizationByTeacherV2Request))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UndoScheduleRealizationByTeacherV2(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/undo-schedule-realization-by-teacher")] HttpRequest req,
            [Queue("notification-sr")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UndoScheduleRealizationByTeacherV2Handler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(ScheduleRealizationV2Endpoint.ResetScheduleRealizationByTeacherV2))]
        [OpenApiOperation(tags: _tag, Summary = "Undo Schedule Realization V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ResetScheduleRealizationByTeacherRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> ResetScheduleRealizationByTeacherV2(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/reset-schedule-realization-by-teacher")] HttpRequest req,
            [Queue("notification-sr")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ResetScheduleRealizationByTeacherHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(ScheduleRealizationV2Endpoint.GetLevelGradeForSubstitution))]
        [OpenApiOperation(tags: _tag, Summary = "Get Level Grade for Substitution")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLevelGradeForSubstitutionRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLevelGradeForSubstitutionRequest.IsLevel), In = ParameterLocation.Query, Type = typeof(bool), Required = true)]
        [OpenApiParameter(nameof(GetLevelGradeForSubstitutionRequest.IdLevel), In = ParameterLocation.Query, Required = false)]

        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLevelForSubstitutionResult))]
        public Task<IActionResult> GetLevelGradeForSubstitution(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-level-grade-for-substitution")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetLevelGradeForSubstitutionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationV2Endpoint.GetTeacherForSubstitutionV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Teacher for Substitution V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTeacherForSubstitutionV2Request.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherForSubstitutionV2Request.IdLevel), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetTeacherForSubstitutionV2Request.IdGrade), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetTeacherForSubstitutionV2Request.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherForSubstitutionV2Request.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherForSubstitutionV2Request.IsSubstituteTeacher), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTeacherForSubstitutionV2Result))]
        public Task<IActionResult> GetTeacherForSubstitutionV2(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-teacher-for-substitution")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTeacherForSubstitutionV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationV2Endpoint.GetSessionVenueForSubstitution))]
        [OpenApiOperation(tags: _tag, Summary = "Get Session Venue for Substitution")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSessionVenueForSubstitutionRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSessionVenueForSubstitutionRequest.IdLevel), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetSessionVenueForSubstitutionRequest.IdGrade), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetSessionVenueForSubstitutionRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSessionVenueForSubstitutionRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSessionVenueForSubstitutionRequest.IdUserTeacher), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetSessionVenueForSubstitutionRequest.IdUserSubstituteTeacher), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetSessionVenueForSubstitutionRequest.IsSession), In = ParameterLocation.Query, Type = typeof(bool), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSessionVenueForSubstitutionResult))]
        public Task<IActionResult> GetSessionVenueForSubstitution(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-session-venue-for-substitution")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSessionVenueForSubstitutionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationV2Endpoint.GetListSubstitutionReportV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Substitution Report V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListSubstitutionReportV2Request.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListSubstitutionReportV2Request.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListSubstitutionReportV2Request.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListSubstitutionReportV2Request.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListSubstitutionReportV2Request.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListSubstitutionReportV2Request.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListSubstitutionReportV2Request.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListSubstitutionReportV2Request.IdLevel), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetListSubstitutionReportV2Request.IdGrade), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetListSubstitutionReportV2Request.StartDate), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetListSubstitutionReportV2Request.EndDate), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetListSubstitutionReportV2Request.IdUserTeacher), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetListSubstitutionReportV2Request.IdUserSubstituteTeacher), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetListSubstitutionReportV2Request.SessionID), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListSubstitutionReportV2Request.IdVenue), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListScheduleRealizationResult))]
        public Task<IActionResult> GetListSubstitutionReportV2(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-substitution-report")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListSubstitutionReporV2tHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationV2Endpoint.SendEmailForCancelClassV2))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email For Cancel Class V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendEmailForCancelClassV2Request))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> SendEmailForCancelClassV2(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/send-email-for-cancel-class")] HttpRequest req,
            [Queue("notification-sr")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SendEmailForCancelClassV2Handler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(ScheduleRealizationV2Endpoint.DownloadSubstitutionReportV2))]
        [OpenApiOperation(tags: _tag, Summary = "Download Substitution Report")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DownloadSubstitutionReportV2Request.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DownloadSubstitutionReportV2Request.IdLevel), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(DownloadSubstitutionReportV2Request.IdGrade), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(DownloadSubstitutionReportV2Request.StartDate), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(DownloadSubstitutionReportV2Request.EndDate), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(DownloadSubstitutionReportV2Request.IdUserTeacher), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(DownloadSubstitutionReportV2Request.IdUserSubstituteTeacher), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(DownloadSubstitutionReportV2Request.SessionID), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(DownloadSubstitutionReportV2Request.IdVenue), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        public Task<IActionResult> DownloadSubstitutionReportV2(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/download-substitution-report")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DownloadSubstitutionReportV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        // [FunctionName(nameof(ScheduleRealizationV2Endpoint.GetStudentAttendance))]
        // [OpenApiOperation(tags: _tag, Summary = "Get Student Attendance")]
        // [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        // [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        // [OpenApiParameter(nameof(GetStudentAttendanceRequest.Search), In = ParameterLocation.Query)]
        // [OpenApiParameter(nameof(GetStudentAttendanceRequest.Return), In = ParameterLocation.Query)]
        // [OpenApiParameter(nameof(GetStudentAttendanceRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        // [OpenApiParameter(nameof(GetStudentAttendanceRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        // [OpenApiParameter(nameof(GetStudentAttendanceRequest.OrderBy), In = ParameterLocation.Query)]
        // [OpenApiParameter(nameof(GetStudentAttendanceRequest.OrderType), In = ParameterLocation.Query)]
        // [OpenApiParameter(nameof(GetStudentAttendanceRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        // [OpenApiParameter(nameof(GetStudentAttendanceRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        // [OpenApiParameter(nameof(GetStudentAttendanceRequest.Date), In = ParameterLocation.Query, Required = true)]
        // [OpenApiParameter(nameof(GetStudentAttendanceRequest.ClassId), In = ParameterLocation.Query, Required = true)]
        // [OpenApiParameter(nameof(GetStudentAttendanceRequest.SessionID), In = ParameterLocation.Query, Required = true)]

        // [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentAttendanceResult))]
        // public Task<IActionResult> GetStudentAttendance(
        //    [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-student-attendance")] HttpRequest req,
        //    CancellationToken cancellationToken)
        // {
        //     var handler = req.HttpContext.RequestServices.GetService<GetStudentAttendanceHandler>();
        //     return handler.Execute(req, cancellationToken);
        // }

        [FunctionName(nameof(ScheduleRealizationV2Endpoint.GetDownloadListScheduleRealizationV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Download List Schedule Realization V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDownloadScheduleRealizationRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]), Required = true)]
        [OpenApiParameter(nameof(GetDownloadScheduleRealizationRequest.IsByDate), In = ParameterLocation.Query, Type = typeof(bool), Required = true)]
        [OpenApiParameter(nameof(GetDownloadScheduleRealizationRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDownloadScheduleRealizationRequest.EndDate), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDownloadScheduleRealizationResult))]
        public Task<IActionResult> GetDownloadListScheduleRealizationV2(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-download-schedule-realization")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDownloadScheduleRealizationV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationV2Endpoint.CheckTeacherOnScheduleRealizationV2))]
        [OpenApiOperation(tags: _tag, Summary = "Check Teacher On Schedule Realization v2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CheckTeacherOnScheduleRealizationV2Request.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(CheckTeacherOnScheduleRealizationV2Request.IdUser), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CheckTeacherOnScheduleRealizationV2Result))]
        public Task<IActionResult> CheckTeacherOnScheduleRealizationV2(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/check-teacher-on-schedule-realization")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CheckTeacherOnScheduleRealizationV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

    }
}
