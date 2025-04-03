using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Data.Models.Binusian.BinusSchool.AttendanceLog;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2
{
    public class AttendanceV2Endpoint
    {
        private const string _route = "attendanceV2";
        private const string _tag = "Attendance List V2";

        #region List Attendance Homeroom Teacher
        [FunctionName(nameof(AttendanceV2Endpoint.GetHomeroomAttendanceV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Homeroom Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceV2Request.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceV2Request.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceV2Request.Semester), In = ParameterLocation.Query, Type = typeof(int), Required = true)]
        [OpenApiParameter(nameof(GetAttendanceV2Request.Date), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(GetAttendanceV2Request.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(HomeroomAttendanceV2Result[]))]
        public Task<IActionResult> GetHomeroomAttendanceV2(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetHomeroomAttendanceHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceV2Endpoint.GetPendingAttendanceV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Pending Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUnresolvedAttendanceV2Request.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUnresolvedAttendanceV2Request.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUnresolvedAttendanceV2Request.CurrentPosition), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUnresolvedAttendanceV2Result))]
        public Task<IActionResult> GetPendingAttendanceV2(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/pending")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetPendingAttendanceV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceV2Endpoint.GetUnsubmittedAttendanceV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Unsubmitted Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUnresolvedAttendanceV2Request.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUnresolvedAttendanceV2Request.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUnresolvedAttendanceV2Request.CurrentPosition), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUnresolvedAttendanceV2Result))]
        public Task<IActionResult> GetUnsubmittedAttendanceV2(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/unsubmitted")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetUnsubmittedAttendanceHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceV2Endpoint.GetUnsubmittedAttendanceEventV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Unsubmitted Attendance Event")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUnsubmittedAttendanceEventV2Request.idUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUnsubmittedAttendanceEventV2Request.idSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUnsubmittedAttendanceEventV2Request.idAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUnsubmittedAttendanceEventV2Result))]
        public Task<IActionResult> GetUnsubmittedAttendanceEventV2(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/unsubmitted-event")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetUnsubmittedAttendanceEventV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceV2Endpoint.GetClassIdByHomeroom))]
        [OpenApiOperation(tags: _tag, Summary = "Get Class Id By Homeroom")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceV2Request.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceV2Request.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceV2Request.Semester), In = ParameterLocation.Query, Type = typeof(int), Required = true)]
        [OpenApiParameter(nameof(GetAttendanceV2Request.Date), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(GetAttendanceV2Request.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClassIdByHomeroomResult[]))]
        public Task<IActionResult> GetClassIdByHomeroom(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route+"/class-id")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetClassIdByHomeroomHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Detail Attendance Homeroom Teacher
        [FunctionName(nameof(AttendanceV2Endpoint.GetAttendanceDetailV2))]
        [OpenApiOperation(tags: _tag, Description = @"
            - ClassId: ClassId from GET /attendance-fn-attendance/class-and-session
            - IdSession: Sessions.Id from GET /attendance-fn-attendance/class-and-session")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceEntryV2Request.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceEntryV2Request.CurrentPosition), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceEntryV2Request.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceEntryV2Request.Date), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(GetAttendanceEntryV2Request.ClassId), In = ParameterLocation.Query, Description = "Fill if AbsentTerm is Session")]
        [OpenApiParameter(nameof(GetAttendanceEntryV2Request.IdSession), In = ParameterLocation.Query, Description = "Fill if AbsentTerm is Session")]
        [OpenApiParameter(nameof(GetAttendanceEntryV2Request.Status), In = ParameterLocation.Query, Description = "Pending|Submitted|Unsubmitted")]
        [OpenApiParameter(nameof(GetAttendanceEntryV2Request.IdHomeroom), In = ParameterLocation.Query, Description = "Pending|Submitted|Unsubmitted")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceEntryV2Result))]
        public Task<IActionResult> GetAttendanceDetailV2(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-entry")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceEntryV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceV2Endpoint.UpdateAttendanceDetailV2))]
        [OpenApiOperation(tags: _tag, Description = @"
            - Id: IdHomeroom from GET /attendance-fn-attendance/attendance-entry
            - ClassId: ClassId from GET /attendance-fn-attendance/class-and-session
            - IdSession: Sessions.Id from GET /attendance-fn-attendance/class-and-session
            - CurrentPosition: Current user assignment, with constant from BinusSchool.Common.Constants.PositionConstant (require when absent term: Session)
            - Entries.IdAttendanceMapAttendance: Attendances.IdAttendanceMapAttendance from GET /attendance-fn-attendance/detail/{idLevel}
            - Entries.File: Payload from POST /attendance-fn-attendance/attendance-entry/upload-file")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateAttendanceEntryV2Request))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateAttendanceDetailV2(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "-entry")] HttpRequest req,
              [Queue("notification-atd-attendance")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateAttendanceEntryV2Handler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }
        #endregion

        #region List Attendance Subject Teacher
        [FunctionName(nameof(AttendanceV2Endpoint.GetSessionAttendanceV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Session Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceV2Request.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceV2Request.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceV2Request.Semester), In = ParameterLocation.Query, Type = typeof(int), Required = true)]
        [OpenApiParameter(nameof(GetAttendanceV2Request.Date), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(GetAttendanceV2Request.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SessionAttendanceV2Result[]))]
        public Task<IActionResult> GetSessionAttendanceV2(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/session")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSessionAttendanceV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceV2Endpoint.UpdateAllAttendanceDetailV2))]
        [OpenApiOperation(tags: _tag, Description = @"
            - Id: IdHomeroom from GET /attendance-fn-attendance/attendance-entry
            - IdSession: Sessions.Id from GET /attendance-fn-attendance/class-and-session
            - CurrentPosition: Current user assignment, with constant from BinusSchool.Common.Constants.PositionConstant (require when absent term: Session)")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateAllAttendanceEntryV2Request))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateAllAttendanceDetailV2(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "-entry/all-present")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateAllAttendanceEntryV2Handler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Attendance Event
        [FunctionName(nameof(AttendanceV2Endpoint.AllPresentEventAttendanceV2))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AllPresentEventAttendanceV2Request))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AllPresentEventAttendanceV2(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-event/all-present")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AllPresentEventAttendanceV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceV2Endpoint.UpdateEventAttendanceDetailV2))]
        [OpenApiOperation(tags: _tag, Description = @"
            - IdEventCheck: From selected tab event check
            - IdLevel: From selected dropdown level
            - Entries.IdAttendanceMapAttendance: Attendances.IdAttendanceMapAttendance from GET /attendance-fn-attendance/detail/{idLevel}
            - Entries.File: Payload from POST /attendance-fn-attendance/attendance-entry/upload-file")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateEventAttendanceEntryV2Request))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateEventAttendanceDetailV2(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "-event")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateEventAttendanceEntryV2Handler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Dashboard
        [FunctionName(nameof(AttendanceV2Endpoint.GetAttendanceUnsubmitedDashboard))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Unsubmited Dashboard")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetAttendanceUnsubmitedDashboardRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceUnsubmitedDashboardResult))]
        public Task<IActionResult> GetAttendanceUnsubmitedDashboard(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/unsubmited-dashboard")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceUnsubmitedDashboardHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Update attendance from Moving
        [FunctionName(nameof(AttendanceV2Endpoint.UpdateAttendanceByMoveStudentEnroll))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateAttendanceByMoveStudentEnrollRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateAttendanceByMoveStudentEnroll(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/move-student")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateAttendanceByMoveStudentEnrollHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Perbaikan Data By Api
        [FunctionName(nameof(AttendanceV2Endpoint.UpdateDoubleData))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(UpdateDoubleDataRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiRequestBody("application/json", typeof(GetAttendanceLogRequest))]

       
        public Task<IActionResult> UpdateDoubleData(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-double-data")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateDoubleDataHandler>();
            return handler.Execute(req, cancellationToken);
        }

        #endregion

        [FunctionName(nameof(AttendanceV2Endpoint.GetAttendanceLogs))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetAttendanceLogRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AttendanceLog[]))]
        public Task<IActionResult> GetAttendanceLogs(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-machine")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceLogMachine>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceV2Endpoint.TestEmail))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceV2Request.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> TestEmail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-email")] HttpRequest req,
            [Queue("notification-atd-attendance")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<TestEmailAttendanceV2Handler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }



    }
}
