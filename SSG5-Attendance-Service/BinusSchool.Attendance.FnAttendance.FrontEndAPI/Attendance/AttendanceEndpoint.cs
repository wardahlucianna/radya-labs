using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.TeacherHomeroomAndSubjectTeacher;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Attendance.FnAttendance.Attendance
{
    public class AttendanceEndpoint
    {
        private const string _route = "attendance";
        private const string _tag = "Attendance List";

        [FunctionName(nameof(AttendanceEndpoint.GetPendingAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Get Pending Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUnresolvedAttendanceRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUnresolvedAttendanceResult))]
        public Task<IActionResult> GetPendingAttendance(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/pending")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetPendingAttendanceHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceEndpoint.GetPendingAttendanceTermDay))]
        [OpenApiOperation(tags: _tag, Summary = "Get Pending Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUnresolvedAttendanceRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUnresolvedAttendanceResult))]
        public Task<IActionResult> GetPendingAttendanceTermDay(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/pending-term-day")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetPendingAttendanceTermDayHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceEndpoint.GetUnsubmittedAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Get Unsubmitted Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUnresolvedAttendanceRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUnresolvedAttendanceRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUnresolvedAttendanceResult))]
        public Task<IActionResult> GetUnsubmittedAttendance(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/unsubmitted")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetUnsubmittedAttendanceHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceEndpoint.GetUnsubmittedAttendanceTermDay))]
        [OpenApiOperation(tags: _tag, Summary = "Get Unsubmitted Attendance Term Day")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUnresolvedAttendanceRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUnresolvedAttendanceRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUnresolvedAttendanceResult))]
        public Task<IActionResult> GetUnsubmittedAttendanceTermDay(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/unsubmitted-term-day")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetUnsubmittedAttendanceTermDayHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceEndpoint.GetUnsubmittedAttendanceEvent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Unsubmitted Attendance Event")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUnsubmittedAttendanceEventRequest.idUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUnsubmittedAttendanceEventRequest.idSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUnsubmittedAttendanceEventRequest.idAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUnresolvedAttendanceResult))]
        public Task<IActionResult> GetUnsubmittedAttendanceEvent(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/unsubmitted-event")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetUnsubmittedAttendanceEventHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceEndpoint.GetHomeroomAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Get Homeroom Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceRequest.Semester), In = ParameterLocation.Query, Type = typeof(int), Required = true)]
        [OpenApiParameter(nameof(GetAttendanceRequest.Date), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(GetAttendanceRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(HomeroomAttendanceResult))]
        public Task<IActionResult> GetHomeroomAttendance(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/homeroom")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetHomeroomAttendanceHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceEndpoint.GetSessionAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Get Session Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceRequest.Semester), In = ParameterLocation.Query, Type = typeof(int), Required = true)]
        [OpenApiParameter(nameof(GetAttendanceRequest.Date), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(GetAttendanceRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SessionAttendanceResult))]
        public Task<IActionResult> GetSessionAttendance(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/session")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSessionAttendanceHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceEndpoint.GetEventAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Get Event Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetEventAttendanceRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEventAttendanceRequest.Date), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(GetEventAttendanceRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetEventAttendanceResult))]
        public Task<IActionResult> GetEventAttendance(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/event")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetEventAttendanceHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceEndpoint.GetConflictedEvent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Conflicted Event")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentConflictedEventsResult))]
        public Task<IActionResult> GetConflictedEvent(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/conflicted-event")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetConflictedEventHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceEndpoint.ChooseConflictedEvent))]
        [OpenApiOperation(tags: _tag, Description = @"
            - ConflictCode: From /attendance-fn-attendance/attendance/conflicted-event
            - IdEventCheck: From selected event check on /attendance-fn-attendance/attendance/conflicted-event")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetEventAttendanceRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEventAttendanceRequest.Date), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(GetEventAttendanceRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiRequestBody("application/json", typeof(ChooseConflictedEventRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ChooseConflictedEvent(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/conflicted-event")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ChooseConflictedEventHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceEndpoint.GetTeacherAssignment))]
        [OpenApiOperation(tags: _tag, Summary = "Teacher Assignment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(TeacherHomeroomAndSubjectTeacherRequest.IdUser), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(TeacherHomeroomAndSubjectTeacherRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<TeacherHomeroomAndSubjectTeacherRequest>))]
        public Task<IActionResult> GetTeacherAssignment(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/teacher-assignment")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<TabbingAttendanceEntryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        #region Master Data Attendance
        [FunctionName(nameof(AttendanceEndpoint.GetAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Master Data List Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMasterDataAttendanceRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMasterDataAttendanceRequest.AttendanceCategory), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterDataAttendanceRequest.AbsenceCategory), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterDataAttendanceRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterDataAttendanceRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterDataAttendanceRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMasterDataAttendanceRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMasterDataAttendanceRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterDataAttendanceRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterDataAttendanceRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMasterDataAttendanceResult))]
        public Task<IActionResult> GetAttendance(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/master-data-attendance")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AttendanceHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(AttendanceEndpoint.GetMasterDataAttendanceDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Master Data Detail Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(MasterDataAttendanceDetailResult))]
        public Task<IActionResult> GetMasterDataAttendanceDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/master-data-attendance/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AttendanceHandler>();
            return handler.Execute(req, cancellationToken, false, "id".WithValue(id));
        }

        [FunctionName(nameof(AttendanceEndpoint.AddMasterDataAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Add Master Data Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMasterDataAttendanceRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddMasterDataAttendance(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/master-data-attendance")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AttendanceHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceEndpoint.UpdateMasterDataAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Update Master Data Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateMasterDataAttendanceRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateMasterDataAttendance(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/master-data-attendance")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AttendanceHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceEndpoint.DeleteMasterAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Master Data Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteMasterAttendance(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/master-data-attendance")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AttendanceHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion
    }
}
