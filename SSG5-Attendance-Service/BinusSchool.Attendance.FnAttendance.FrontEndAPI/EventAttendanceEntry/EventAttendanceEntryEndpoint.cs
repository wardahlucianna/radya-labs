using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceEntry;
using BinusSchool.Data.Model.Attendance.FnAttendance.EventAttendanceEntry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Attendance.FnAttendance.EventAttendanceEntry
{
    public class EventAttendanceEntryEndpoint
    {
        private const string _route = "event-attendance-entry";
        private const string _tag = "Event Attendance Entry";

        [FunctionName(nameof(EventAttendanceEntryEndpoint.GetEventAttendanceInformation))]
        [OpenApiOperation(tags: _tag, Description = @"For fill event information and get dropdown date from available dates")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetEventAttendanceInformationRequest.IdEvent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(EventAttendanceInformationResult))]
        public Task<IActionResult> GetEventAttendanceInformation(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/information")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetEventAttendanceInformationHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EventAttendanceEntryEndpoint.GetEventAttendanceCheck))]
        [OpenApiOperation(tags: _tag, Description = @"For tab attandance check
            - Date: From GET /attendance-fn-attendance/event-attendance-entry available dates, choose selected date")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetEventAttendanceCheckRequest.IdEvent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEventAttendanceCheckRequest.Date), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(EventCheck))]
        public Task<IActionResult> GetEventAttendanceCheck(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-check")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetEventAttendanceCheckHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EventAttendanceEntryEndpoint.GetLevelsByEvent))]
        [OpenApiOperation(tags: _tag, Description = @"For dropdown level, required on any option type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLevelsByEventRequest.IdEvent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLevelsByEventRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CodeWithIdVm))]
        public Task<IActionResult> GetLevelsByEvent(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-level")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetLevelsByEventHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EventAttendanceEntryEndpoint.GetGradesByEvent))]
        [OpenApiOperation(tags: _tag, Description = @"For dropdown grade, visible on any option type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGradesByEventRequest.IdEvent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGradesByEventRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGradesByEventRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CodeWithIdVm))]
        public Task<IActionResult> GetGradesByEvent(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-grade")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGradesByEventHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EventAttendanceEntryEndpoint.GetSubjectsByEvent))]
        [OpenApiOperation(tags: _tag, Description = @"For subject grade, only visible on event with subject option type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSubjectsByEventRequest.IdEvent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSubjectsByEventRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSubjectsByEventRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CodeWithIdVm))]
        public Task<IActionResult> GetSubjectsByEvent(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-subject")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSubjectsByEventHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EventAttendanceEntryEndpoint.GetHomeroomsByEvent))]
        [OpenApiOperation(tags: _tag, Description = @"For homeroom dropdown, only visible on event with grade option type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetHomeroomsByEventRequest.IdEvent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetHomeroomsByEventRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetHomeroomsByEventRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm))]
        public Task<IActionResult> GetHomeroomsByEvent(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-homeroom")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetHomeroomsByEventHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EventAttendanceEntryEndpoint.GetEventAttendanceEntry))]
        [OpenApiOperation(tags: _tag, Description = @"For tab attandance entry list
            - IdEventCheck: From selected tab event check
            - IdLevel: From selected dropdown level
            - IdGrade: From selected dropdown grade
            - IdHomeroom: From selected dropdown homeroom
            - IdSubject: From selected subject dropdown
            - IsSubmitted: From selected status dropdown (All = null,Submitted = true,Unsubmitted = false)")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetEventAttendanceEntryRequest.IdEventCheck), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEventAttendanceEntryRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEventAttendanceEntryRequest.Date), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEventAttendanceEntryRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEventAttendanceEntryRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEventAttendanceEntryRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEventAttendanceEntryRequest.IdEvent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEventAttendanceEntryRequest.IdUser), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEventAttendanceEntryRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEventAttendanceEntryRequest.IsSubmitted), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(EventAttendanceEntryResult))]
        public Task<IActionResult> GetEventAttendanceEntry(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetEventAttendanceEntryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EventAttendanceEntryEndpoint.GetEventAttendanceSummary))]
        [OpenApiOperation(tags: _tag, Description = @"For tab attandance summary list
            - IdEventCheck: From selected tab event check
            - IdLevel: From selected dropdown level
            - IdGrade: From selected dropdown grade
            - IdHomeroom: From selected dropdown homeroom
            - IdSubject: From selected subject dropdown")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetEventAttendanceSummaryRequest.IdEventCheck), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEventAttendanceSummaryRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEventAttendanceSummaryRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEventAttendanceSummaryRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEventAttendanceSummaryRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(EventAttendanceSummaryResult))]
        public Task<IActionResult> GetEventAttendanceSummary(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/summary")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetEventAttendanceSummaryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EventAttendanceEntryEndpoint.UploadFileEventAttendanceEntry))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(UploadFileEventAttendanceEntryRequest.IdEventCheck), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(UploadFileEventAttendanceEntryRequest.IdUserEvent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(string))]
        public Task<IActionResult> UploadFileEventAttendanceEntry(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/upload-file")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UploadFileEventAttendanceEntryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EventAttendanceEntryEndpoint.DownloadFileEventAttendanceEntry))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DownloadFileAttendanceEntryRequest.FileName), In = ParameterLocation.Query, Required = true)]
        public Task<IActionResult> DownloadFileEventAttendanceEntry(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/download-file")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DownloadFileEventAttendanceEntryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EventAttendanceEntryEndpoint.UpdateEventAttendanceDetail))]
        [OpenApiOperation(tags: _tag, Description = @"
            - IdEventCheck: From selected tab event check
            - IdLevel: From selected dropdown level
            - Entries.IdAttendanceMapAttendance: Attendances.IdAttendanceMapAttendance from GET /attendance-fn-attendance/detail/{idLevel}
            - Entries.File: Payload from POST /attendance-fn-attendance/attendance-entry/upload-file")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateEventAttendanceEntryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateEventAttendanceDetail(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateEventAttendanceEntryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EventAttendanceEntryEndpoint.AllPresentEventAttendance))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AllPresentEventAttendanceRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AllPresentEventAttendance(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/all-present")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AllPresentEventAttendanceHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EventAttendanceEntryEndpoint.AllPresentExcuseAbsentEventAttendance))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AllPresentAllExcuseEventAttendanceRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AllPresentExcuseAbsentEventAttendance(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/all-present-excuse-absent")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AllPresentExcuseAbsentHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
