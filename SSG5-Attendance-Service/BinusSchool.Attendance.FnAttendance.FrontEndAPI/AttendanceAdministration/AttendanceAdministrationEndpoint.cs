using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceAdministrationV2;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.ApprovalAttendanceAdministration;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministration
{
    public class AttendanceAdministrationEndpoint
    {
        private const string _route = "attendance-administration";
        private const string _tag = "Attendance Administration";
        private readonly AttendanceAdministrationHandler _handler;
        private readonly AttendanceAdmnistrationSummaryHandler _summaryHandler;
        private readonly AttendanceAdministrationGetStudentHandler _attendanceAdministrationGetStudentHandler;
        private readonly AttendanceAdministrationGetHomeroomHandler _attendanceAdministrationGetHomeroomHandler;
        private readonly AttendanceAdministrationGetSubjectHandler _attendanceAdministrationGetSubjectHandler;
        private readonly ListAttendanceAdministrationApprovalHandler _listAttendanceAdministrationApprovalHandler;
        public AttendanceAdministrationEndpoint(AttendanceAdministrationHandler handler, AttendanceAdmnistrationSummaryHandler summaryHandler, AttendanceAdministrationGetStudentHandler attendanceAdministrationGetStudentHandler, AttendanceAdministrationGetHomeroomHandler attendanceAdministrationGetHomeroomHandler, AttendanceAdministrationGetSubjectHandler attendanceAdministrationGetSubjectHandler)
        {
            _handler = handler;
            _summaryHandler = summaryHandler;
            _attendanceAdministrationGetStudentHandler = attendanceAdministrationGetStudentHandler;
            _attendanceAdministrationGetHomeroomHandler = attendanceAdministrationGetHomeroomHandler;
            _attendanceAdministrationGetSubjectHandler = attendanceAdministrationGetSubjectHandler;
        }

        [FunctionName(nameof(AttendanceAdministrationEndpoint.GetAttendanceAdministration))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceAdministrationRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceAdministrationRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceAdministrationRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAttendanceAdministrationRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAttendanceAdministrationRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceAdministrationRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceAdministrationRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetAttendanceAdministrationRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceAdministrationRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceAdministrationResult))]
        public Task<IActionResult> GetAttendanceAdministration(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceAdministrationEndpoint.GetAttendanceAdministrationDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Attendance Administration Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceAdministrationDetailResult))]
        public Task<IActionResult> GetAttendanceAdministrationDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(AttendanceAdministrationEndpoint.AttendanceAdministrationSummary))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Attendance Administration")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetAttendanceAdministrationSummaryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AttendanceAdministrationSummary(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-summary")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _summaryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceAdministrationEndpoint.AddAttendanceAdministration))]
        [OpenApiOperation(tags: _tag, Summary = "Add Attendance Administration")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(PostAttendanceAdministrationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddAttendanceAdministration(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
           [Queue("notification-atd-attendance")] ICollector<string> collector,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, false, nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(AttendanceAdministrationEndpoint.GetStudents))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAdministrationAttendanceStudentRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAdministrationAttendanceStudentRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAdministrationAttendanceStudentRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAdministrationAttendanceStudentRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAdministrationAttendanceStudentRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAdministrationAttendanceStudentRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAdministrationAttendanceStudentRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetAdministrationAttendanceStudentRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAdministrationAttendanceStudentRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAdministrationAttendanceStudentRequest.IdPathway), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAdministrationAttendanceStudentRequest.IdHomeroom), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetAdministrationAttendanceStudentRequest.IdSubject), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetAdministrationAttendanceStudentRequest.IdStudent), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ICollection<GetAdministrationAttendanceStudentResult>))]
        public Task<IActionResult> GetStudents(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-get-students")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _attendanceAdministrationGetStudentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceAdministrationEndpoint.GetHomeroom))]
        [OpenApiOperation(tags: _tag, Summary = "Get Homeroom List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAdministrationAttendanceHomeroomRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAdministrationAttendanceHomeroomRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ICollection<CodeWithIdVm>))]
        public Task<IActionResult> GetHomeroom(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-get-homeroom")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _attendanceAdministrationGetHomeroomHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceAdministrationEndpoint.GetSubject))]
        [OpenApiOperation(tags: _tag, Summary = "Get Subject List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAdministrationAttendanceSubjectRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAdministrationAttendanceSubjectRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ICollection<CodeWithIdVm>))]
        public Task<IActionResult> GetSubject(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-get-subject")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _attendanceAdministrationGetSubjectHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceAdministrationEndpoint.GetAttendanceAdministrationApproval))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Attendance Administration Approval")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalRequest.Semester), In = ParameterLocation.Query, Type = typeof(int), Required = true)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalRequest.AttendanceCategory), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalRequest.Status), In = ParameterLocation.Query, Type = typeof(int))]
        
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ICollection<GetListAttendanceAdministrationApprovalResult>))]
        public Task<IActionResult> GetAttendanceAdministrationApproval(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-list-approval")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ListAttendanceAdministrationApprovalHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceAdministrationEndpoint.SetStatusApprovalAttendanceAdministration))]
        [OpenApiOperation(tags: _tag, Summary = "Set Status Approval Attendance Administration")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SetStatusApprovalAttendanceAdministrationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SetStatusApprovalAttendanceAdministration(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "-set-status-approval")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SetStatusApprovalAttendanceAdministrationHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
