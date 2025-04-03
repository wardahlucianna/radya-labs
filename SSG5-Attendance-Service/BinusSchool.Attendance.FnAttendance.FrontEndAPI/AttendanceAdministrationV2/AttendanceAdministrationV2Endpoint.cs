using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministrationV2;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministrationV2
{
    public class AttendanceAdministrationV2Endpoint
    {
        private const string _route = "attendance-administrationV2";
        private const string _tag = "Attendance Administration V2";
        private readonly AttendanceAdministrationV2Handler _handler;
        public AttendanceAdministrationV2Endpoint(AttendanceAdministrationV2Handler handler)
        {
            _handler = handler;
        }

        [FunctionName(nameof(AttendanceAdministrationV2Endpoint.GetAttendanceAdministrationV2))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceAdministrationV2Request.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceAdministrationV2Request.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceAdministrationV2Request.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAttendanceAdministrationV2Request.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAttendanceAdministrationV2Request.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceAdministrationV2Request.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceAdministrationV2Request.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetAttendanceAdministrationV2Request.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceAdministrationV2Request.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceAdministrationV2Request.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceAdministrationV2Request.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceAdministrationV2Request.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceAdministrationV2Request.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceAdministrationV2Result))]
        public Task<IActionResult> GetAttendanceAdministrationV2(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceAdministrationV2Endpoint.GetAttendanceAdministrationDetailV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Attendance Administration Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceAdministrationDetailV2Result))]
        public Task<IActionResult> GetAttendanceAdministrationDetailV2(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(AttendanceAdministrationV2Endpoint.AddAttendanceAdministrationV2))]
        [OpenApiOperation(tags: _tag, Summary = "Add Attendance Administration")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(PostAttendanceAdministrationV2Request))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddAttendanceAdministrationV2(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
           [Queue("notification-atd-attendance")] ICollector<string> collector,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(AttendanceAdministrationV2Endpoint.EditAttendanceAdministrationV2))]
        [OpenApiOperation(tags: _tag, Summary = "Edit Attendance Administration")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(PutAttendanceAdministrationV2Request))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> EditAttendanceAdministrationV2(
       [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
       [Queue("notification-atd-attendance")] ICollector<string> collector,
       CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(AttendanceAdministrationV2Endpoint.GetAttendanceAdministrationApprovalV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Attendance Administration Approval")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalV2Request.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalV2Request.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalV2Request.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalV2Request.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalV2Request.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalV2Request.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalV2Request.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalV2Request.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalV2Request.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalV2Request.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalV2Request.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalV2Request.Semester), In = ParameterLocation.Query, Type = typeof(int), Required = true)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalV2Request.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalV2Request.AttendanceCategory), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListAttendanceAdministrationApprovalV2Request.Status), In = ParameterLocation.Query, Type = typeof(int))]
        
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ICollection<GetListAttendanceAdministrationApprovalV2Result>))]
        public Task<IActionResult> GetAttendanceAdministrationApprovalV2(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-list-approval")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ListAttendanceAdministrationApprovalV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceAdministrationV2Endpoint.SetStatusApprovalAttendanceAdministrationV2))]
        [OpenApiOperation(tags: _tag, Summary = "Set Status Approval Attendance Administration")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SetStatusApprovalAttendanceAdministrationV2Request))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SetStatusApprovalAttendanceAdministrationV2(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "-set-status-approval")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SetStatusApprovalAttendanceAdministrationV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceAdministrationV2Endpoint.GetSubjectV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Subject List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAdministrationAttendanceSubjectRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAdministrationAttendanceSubjectRequest.IdUserStudent), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetAdministrationAttendanceSubjectRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ICollection<CodeWithIdVm>))]
        public Task<IActionResult> GetSubjectV2(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-get-subject")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AttendanceAdministrationGetSubjectV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        #region Undu Attendance
        [FunctionName(nameof(AttendanceAdministrationV2Endpoint.GetCancelAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Get Subject List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCancelAttendanceRequest.IdAttendanceAdministration), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ICollection<GetCancelAttendanceResult>))]
        public Task<IActionResult> GetCancelAttendance(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-cancel-attendance")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetCancelAttendanceHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceAdministrationV2Endpoint.CancelAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Get Subject List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CancelAttendanceRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> CancelAttendance(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-cancel-attendance")] HttpRequest req,
         [Queue("notification-atd-attendance")] ICollector<string> collector,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CancelAttendanceHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }
        #endregion
    }
}
