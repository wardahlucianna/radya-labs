using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization
{
    public class ScheduleRealizationEndpoint
    {
        private const string _route = "schedule/schedule-realization";
        private const string _tag = "Schedule Realization";

        [FunctionName(nameof(ScheduleRealizationEndpoint.GetTeacherByDateAY))]
        [OpenApiOperation(tags: _tag, Summary = "Get Teacher by Date AY,Level,Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTeacherByDateAYRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherByDateAYRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherByDateAYRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherByDateAYRequest.IdLevel), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetTeacherByDateAYRequest.IdGrade), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetTeacherByDateAYRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherByDateAYRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTeacherByDateAYResult))]
        public Task<IActionResult> GetTeacherByDateAY(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-teacher-by-date-ay")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTeacherByDateAYHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationEndpoint.GetSessionByTeacherDate))]
        [OpenApiOperation(tags: _tag, Summary = "Get Session by Date AY,Teacher,Level,Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSessionByTeacherDateReq.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSessionByTeacherDateReq.IdUser), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetSessionByTeacherDateReq.IdLevel), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetSessionByTeacherDateReq.IdGrade), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetSessionByTeacherDateReq.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSessionByTeacherDateReq.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSessionByTeacherDateResult))]
        public Task<IActionResult> GetSessionByTeacherDate(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-session-by-teacher-date")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSessionByTeacherDateHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationEndpoint.GetVenueByTeacherDate))]
        [OpenApiOperation(tags: _tag, Summary = "Get Venue by Date AY,Teacher,Level,Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetVenueByTeacherDateRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetVenueByTeacherDateRequest.IdUser), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetVenueByTeacherDateRequest.IdLevel), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetVenueByTeacherDateRequest.IdGrade), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetVenueByTeacherDateRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetVenueByTeacherDateRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetVenueByTeacherDateResult))]
        public Task<IActionResult> GetVenueByTeacherDate(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-venue-by-teacher-date")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetVenueByTeacherDateHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationEndpoint.GetListScheduleRealization))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Schedule Realization")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListScheduleRealizationRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListScheduleRealizationRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListScheduleRealizationRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListScheduleRealizationRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListScheduleRealizationRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListScheduleRealizationRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListScheduleRealizationRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListScheduleRealizationRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListScheduleRealizationRequest.IdGrade), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetListScheduleRealizationRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListScheduleRealizationRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListScheduleRealizationRequest.IdUserTeacher), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetListScheduleRealizationRequest.SessionID), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListScheduleRealizationRequest.IdVenue), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListScheduleRealizationResult))]
        public Task<IActionResult> GetListScheduleRealization(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-schedule-realization")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListScheduleRealizationHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationEndpoint.SaveScheduleRealization))]
        [OpenApiOperation(tags: _tag, Summary = "Save Schedule Realization")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveScheduleRealizationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> SaveScheduleRealization(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-schedule-realization")] HttpRequest req,
            [Queue("notification-sr")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SaveScheduleRealizationHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(ScheduleRealizationEndpoint.GetClassIdByTeacherDate))]
        [OpenApiOperation(tags: _tag, Summary = "Get Class ID by Teacher Date AY")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetClassIdByTeacherDateRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassIdByTeacherDateRequest.idUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassIdByTeacherDateRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassIdByTeacherDateRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClassIdByTeacherDateResult))]
        public Task<IActionResult> GetClassIdByTeacherDate(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-class-id-by-teacher-date")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetClassIdBYTeacherDateHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationEndpoint.GetListScheduleRealizationByTeacher))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Schedule Realization")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherRequest.IdUserTeacher), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListScheduleRealizationByTeacherRequest.ClassID), In = ParameterLocation.Query, Type = typeof(string[]), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListScheduleRealizationByTeacherResult))]
        public Task<IActionResult> GetListScheduleRealizationByTeacher(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-schedule-realization-by-teacher")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListScheduleRealizationByTeacherHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationEndpoint.SaveScheduleRealizationByTeacher))]
        [OpenApiOperation(tags: _tag, Summary = "Save Schedule Realization by Teacher")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveScheduleRealizationByTeacherRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> SaveScheduleRealizationByTeacher(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-schedule-realization-by-teacher")] HttpRequest req,
            [Queue("notification-sr")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SaveScheduleRealizationByTeacherHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(ScheduleRealizationEndpoint.GetLevelForSubstitution))]
        [OpenApiOperation(tags: _tag, Summary = "Get Level for Substitution")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLevelForSubstitutionRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLevelForSubstitutionResult))]
        public Task<IActionResult> GetLevelForSubstitution(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-level-for-substitution")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetLevelForSubstitutionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationEndpoint.GetGradeForSubstitution))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade for Substitution")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGradeForSubstitutionRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGradeForSubstitutionRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGradeForSubstitutionResult))]
        public Task<IActionResult> GetGradeForSubstitution(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-grade-for-substitution")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGradeForSubstitutionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationEndpoint.GetTeacherForSubstitution))]
        [OpenApiOperation(tags: _tag, Summary = "Get Teacher for Substitution")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTeacherForSubstitutionRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherForSubstitutionRequest.IdLevel), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetTeacherForSubstitutionRequest.IdGrade), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetTeacherForSubstitutionRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherForSubstitutionRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherForSubstitutionRequest.IsSubstituteTeacher), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTeacherForSubstitutionResult))]
        public Task<IActionResult> GetTeacherForSubstitution(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-teacher-for-substitution")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTeacherForSubstitutionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationEndpoint.GetSessionForSubstitution))]
        [OpenApiOperation(tags: _tag, Summary = "Get Session for Substitution")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSessionForSubstitutionRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSessionForSubstitutionRequest.IdLevel), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetSessionForSubstitutionRequest.IdGrade), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetSessionForSubstitutionRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSessionForSubstitutionRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSessionForSubstitutionRequest.IdUserTeacher), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetSessionForSubstitutionRequest.IdUserSubstituteTeacher), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSessionForSubstitutionResult))]
        public Task<IActionResult> GetSessionForSubstitution(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-session-for-substitution")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSessionForSubstitutionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationEndpoint.GetVenueForSubstitution))]
        [OpenApiOperation(tags: _tag, Summary = "Get Venue for Substitution")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetVenueForSubstitutionRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetVenueForSubstitutionRequest.IdLevel), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetVenueForSubstitutionRequest.IdGrade), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetVenueForSubstitutionRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetVenueForSubstitutionRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetVenueForSubstitutionRequest.IdUserTeacher), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetVenueForSubstitutionRequest.IdUserSubstituteTeacher), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetVenueForSubstitutionResult))]
        public Task<IActionResult> GetVenueForSubstitution(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-venue-for-substitution")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetVenueForSubstitutionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationEndpoint.GetListSubstitutionReport))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Schedule Realization")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListSubstitutionReportRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListSubstitutionReportRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListSubstitutionReportRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListSubstitutionReportRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListSubstitutionReportRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListSubstitutionReportRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListSubstitutionReportRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListSubstitutionReportRequest.IdLevel), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetListSubstitutionReportRequest.IdGrade), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetListSubstitutionReportRequest.StartDate), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetListSubstitutionReportRequest.EndDate), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetListSubstitutionReportRequest.IdUserTeacher), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetListSubstitutionReportRequest.IdUserSubstituteTeacher), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(GetListSubstitutionReportRequest.SessionID), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListSubstitutionReportRequest.IdVenue), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListScheduleRealizationResult))]
        public Task<IActionResult> GetListSubstitutionReport(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-substitution-report")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListSubstitutionReportHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationEndpoint.GetSettingEmailScheduleRealization))]
        [OpenApiOperation(tags: _tag, Summary = "Get Setting Email Schedule Realization")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSettingEmailScheduleRealizationRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSettingEmailScheduleRealizationResult))]
        public Task<IActionResult> GetSettingEmailScheduleRealization(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-setting-email-schedule-realization")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSettingEmailScheduleRealizationHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationEndpoint.SaveSettingEmailScheduleRealization))]
        [OpenApiOperation(tags: _tag, Summary = "Save Setting Email Schedule Realization")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveSettingEmailScheduleRealizationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> SaveSettingEmailScheduleRealization(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-setting-email-schedule-realization")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SaveSettingEmailScheduleRealizationHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationEndpoint.SendEmailForCancelClass))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email For Cancel Class")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendEmailForCancelClassRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> SendEmailForCancelClass(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/send-email-for-cancel-class")] HttpRequest req,
            [Queue("notification-sr")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SendEmailForCancelClassHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(ScheduleRealizationEndpoint.DownloadSubstitutionReport))]
        [OpenApiOperation(tags: _tag, Summary = "Download Substitution Report")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DownloadSubstitutionReportRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DownloadSubstitutionReportRequest.IdLevel), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(DownloadSubstitutionReportRequest.IdGrade), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(DownloadSubstitutionReportRequest.StartDate), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(DownloadSubstitutionReportRequest.EndDate), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(DownloadSubstitutionReportRequest.IdUserTeacher), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(DownloadSubstitutionReportRequest.IdUserSubstituteTeacher), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        [OpenApiParameter(nameof(DownloadSubstitutionReportRequest.SessionID), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(DownloadSubstitutionReportRequest.IdVenue), In = ParameterLocation.Query, Type = typeof(string[]), Required = false)]
        public Task<IActionResult> DownloadSubstitutionReport(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/download-substitution-report")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DownloadSubstitutionReportHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationEndpoint.GetStudentAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentAttendanceRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentAttendanceRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentAttendanceRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentAttendanceRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentAttendanceRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentAttendanceRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentAttendanceRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentAttendanceRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentAttendanceRequest.Date), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentAttendanceRequest.ClassId), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentAttendanceRequest.SessionID), In = ParameterLocation.Query, Required = true)]

        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentAttendanceResult))]
        public Task<IActionResult> GetStudentAttendance(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-student-attendance")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentAttendanceHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationEndpoint.GetDownloadListScheduleRealization))]
        [OpenApiOperation(tags: _tag, Summary = "Get Download List Schedule Realization")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDownloadScheduleRealizationRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]), Required = true)]
        [OpenApiParameter(nameof(GetDownloadScheduleRealizationRequest.IsByDate), In = ParameterLocation.Query, Type = typeof(bool), Required = true)]
        [OpenApiParameter(nameof(GetDownloadScheduleRealizationRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDownloadScheduleRealizationRequest.EndDate), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDownloadScheduleRealizationResult))]
        public Task<IActionResult> GetDownloadListScheduleRealization(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-download-schedule-realization")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDownloadScheduleRealizationHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleRealizationEndpoint.CheckTeacherOnScheduleRealization))]
        [OpenApiOperation(tags: _tag, Summary = "Check Teacher On Schedule Realization")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CheckTeacherOnScheduleRealizationRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(CheckTeacherOnScheduleRealizationRequest.IdUser), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CheckTeacherOnScheduleRealizationResult))]
        public Task<IActionResult> CheckTeacherOnScheduleRealization(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/check-teacher-on-schedule-realization")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CheckTeacherOnScheduleRealizationHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
