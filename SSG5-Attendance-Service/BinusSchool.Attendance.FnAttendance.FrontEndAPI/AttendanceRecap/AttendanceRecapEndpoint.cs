using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs;
using Microsoft.OpenApi.Models;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceRecap;
using BinusSchool.Attendance.FnAttendance.AttendanceAdministrationV2;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministrationV2;
using BinusSchool.Common.Extensions;
using BinusSchool.Attendance.FnAttendance.AttendanceEntry;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;

namespace BinusSchool.Attendance.FnAttendance.AttendanceRecap
{
    public class AttendanceRecapEndpoint
    {
        private const string _route = "attendance-recap";
        private const string _tag = "Attendance Recap";
        private readonly AttendanceRecapHandler _handler;

        public AttendanceRecapEndpoint(AttendanceRecapHandler handler)
        {
            _handler = handler;
        }

        [FunctionName(nameof(AttendanceRecapEndpoint.GetAttendanaceRecap))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceRecapRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceRecapRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceRecapRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAttendanceRecapRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAttendanceRecapRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceRecapRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceRecapRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetAttendanceRecapRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceRecapRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceRecapRequest.IdStudent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceRecapRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceRecapRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceRecapRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceRecapResult[]))]
        public Task<IActionResult> GetAttendanaceRecap(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceRecapEndpoint.GetDetailAttendanceRecap))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceRecapResult))]
        public Task<IActionResult> GetDetailAttendanceRecap(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailAttendanceRecapHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceRecapEndpoint.GetUnsubmittedAttendanceRecap))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryUnsubmitedResult[]))]
        public Task<IActionResult> GetUnsubmittedAttendanceRecap(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/unsubmitted")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetUnsubmittedAttendaceRecapHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceRecapEndpoint.GetPendingAttendanceRecap))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDataDetailAttendanceRecapResult[]))]
        public Task<IActionResult> GetPendingAttendanceRecap(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/pending")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetPendingAttendanceRecapHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceRecapEndpoint.GetPresentAttendanceRecap))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDataDetailAttendanceRecapResult[]))]
        public Task<IActionResult> GetPresentAttendanceRecap(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/present")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetPresentAttendanceRecapHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceRecapEndpoint.GetLateAttendanceRecap))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDataDetailAttendanceRecapResult[]))]
        public Task<IActionResult> GetLateAttendanceRecap(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/late")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetLateAttendanceRecapHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceRecapEndpoint.GetExcusedAttendanceRecap))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDataDetailAttendanceRecapResult[]))]
        public Task<IActionResult> GetExcusedAttendanceRecap(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/excused")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetExcusedAttendanceRecapHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceRecapEndpoint.GetUnexcusedAttendanceRecap))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDataDetailAttendanceRecapResult[]))]
        public Task<IActionResult> GetUnexcusedAttendanceRecap(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/unexcused")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetUnexcusedAttendanceRecapHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceRecapEndpoint.GetDownloadAttendanceRecap))]
        [OpenApiOperation(tags: _tag, Summary = "Get Download Attendance Recap By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceRecapRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AttendanceRecapData[]))]
        public Task<IActionResult> GetDownloadAttendanceRecap(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/download")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDownloadAttendanceRecapHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceRecapEndpoint.GetLatesUpdateAttendanceRecap))]
        [OpenApiOperation(tags: _tag, Summary = "Get Lates Update Attendance Recap")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DateTime))]
        public Task<IActionResult> GetLatesUpdateAttendanceRecap(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/lates-update")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetLatesUpdateAttendanceRecapHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
