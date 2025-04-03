using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class AttendanceSummaryTermEndpoint
    {
        private const string _route = "attendance-summary-term";
        private const string _tag = "Attendance Summary Term";

        #region Attendance Summary
        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummary))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryRequest.SelectedPosition), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryResult[]))]
        public Task<IActionResult> GetAttendanceSummary(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryWidge))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Widge")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryRequest.SelectedPosition), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryWidgeResult))]
        public Task<IActionResult> GetAttendanceSummaryWidge(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/widge")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryWidgeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryUnsubmited))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Unsubmited")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.SelectedPosition), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdClassroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.StartDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.EndDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.ClassId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.SchoolId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.SearchBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.GetAll), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryUnsubmitedResult))]
        public Task<IActionResult> GetAttendanceSummaryUnsubmited(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/unsubmited")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            //var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryUnsubmitedHandler>();
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryUnsubmitedHandler2>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryPending))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Pending")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryPendingRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryPendingRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryPendingRequest.SelectedPosition), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryPendingRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryPendingRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryPendingRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryPendingRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryPendingRequest.IdClassroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryPendingRequest.StartDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryPendingRequest.EndDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.SearchBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.GetAll), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryPendingResult))]
        public Task<IActionResult> GetAttendanceSummaryPending(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/pending")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryPendingHandler2>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetLessonByPosition))]
        [OpenApiOperation(tags: _tag, Summary = "Get Lesson By Position")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.SelectedPosition), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdClassroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.ClassId), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(string[]))]
        public Task<IActionResult> GetLessonByPosition(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/lesson-by-position")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetLessonByPositionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetDownloadAttendanceSummaryPending))]
        [OpenApiOperation(tags: _tag, Summary = "Get Download Attendance Summary Pending")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDownloadAttendanceSummaryPendingRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDownloadAttendanceSummaryPendingRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDownloadAttendanceSummaryPendingRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDownloadAttendanceSummaryPendingRequest.SelectedPosition), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDownloadAttendanceSummaryPendingRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDownloadAttendanceSummaryPendingRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> GetDownloadAttendanceSummaryPending(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/pending-download")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDownloadAttendanceSummaryPendingHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Attendance summary detail (student)
        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.SelectedPosition), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.IdClassroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.Measure), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.AttendanceType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.Percent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.IdPeriod), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryDetailResult[]))]
        public Task<IActionResult> GetAttendanceSummaryDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryDetailHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryDetailWidge))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail Widge By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.SelectedPosition), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.IdPeriod), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.IdClassroom), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryDetailWidgeResult))]
        public Task<IActionResult> GetAttendanceSummaryDetailWidge(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail/widge")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryDetailWidgeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryHomeroom))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Attendance Homeroom")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryHomeroomRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryHomeroomRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryHomeroomRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryHomeroomRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryHomeroomResult))]
        public Task<IActionResult> GetAttendanceSummaryHomeroom(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/homeroom")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryHomeroomHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetDownloadAllUnexcusedAbsenceByTerm))]
        [OpenApiOperation(tags: _tag, Summary = "Get Download All Unexcused Absence")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByTermRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByTermRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByTermRequest.IdPeriod), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByTermRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByTermRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByTermRequest.IdClassroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByTermRequest.ClassId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByTermRequest.IdSession), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByTermRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByTermRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByTermRequest.IsDailyAttendance), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> GetDownloadAllUnexcusedAbsenceByTerm(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-download-all-unexcused-absence-by-term")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDownloadAllUnexcusedAbsenceByTermHandler2>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetDownloadAttendanceSummary))]
        [OpenApiOperation(tags: _tag, Summary = "Get Download Attendance Summary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.SelectedPosition), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.IdClassroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.Measure), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.AttendanceType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.Percent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.IdPeriod), In = ParameterLocation.Query)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> GetDownloadAttendanceSummary(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/download")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDownloadAttendanceSummaryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryDetailAttendanceRate))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail Attendance Rate By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailAttendanceRateRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailAttendanceRateRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailAttendanceRateRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailAttendanceRateRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailAttendanceRateRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailAttendanceRateRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailAttendanceRateRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailAttendanceRateRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailAttendanceRateRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailAttendanceRateRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailAttendanceRateRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryDetailAttendanceRateResult))]
        public Task<IActionResult> GetAttendanceSummaryDetailAttendanceRate(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail/attendance-rate")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryDetailAttendanceRateHandler2>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryDetailAttendanceRateDownload))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail Attendance Rate Download By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailAttendanceRateRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailAttendanceRateRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailAttendanceRateRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailAttendanceRateRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailAttendanceRateRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailAttendanceRateRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> GetAttendanceSummaryDetailAttendanceRateDownload(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail/attendance-rate/download")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryDetailAttendanceRateDownloadHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryDetailUnexcusedExcused))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail Unexcused Excused By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.AbsenceCategory), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.ExcuseAbsenceCategory), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryDetailUnexcusedExcusedResult))]
        public Task<IActionResult> GetAttendanceSummaryDetailUnexcusedExcused(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail/unexcused-excused")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryDetailUnexcusedExcusedHandler2>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryDetailUnexcusedExcusedDownload))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail Unexcused Excused Download By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.AbsenceCategory), In = ParameterLocation.Query)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> GetAttendanceSummaryDetailUnexcusedExcusedDownload(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail/unexcused-excused/download")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryDetailUnexcusedExcusedDownloadHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryDetailUnexcusedExcusedSummary))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail Unexcused Excused By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailUnexcusedExcusedRequest.AbsenceCategory), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryDetailUnexcusedExcusedSummaryResult))]
        public Task<IActionResult> GetAttendanceSummaryDetailUnexcusedExcusedSummary(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail/unexcused-excused/summary")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryDetailUnexcusedExcusedSummaryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryDetailWorkhabit))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail lWorkhabit")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailWorkhabitRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailWorkhabitRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailWorkhabitRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailWorkhabitRequest.IdMappingAttendanceWorkhabit), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailWorkhabitRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailWorkhabitRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailWorkhabitRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryDetailWorkhabitResult))]
        public Task<IActionResult> GetAttendanceSummaryDetailWorkhabit(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail/workhabit")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryDetailWorkhabitHandler2>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Attendance summary detail (Level)
        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryDetailLevel))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail By Level")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.SelectedPosition), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.IdClassroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.IdPeriod), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryDetailLevelResult[]))]
        public Task<IActionResult> GetAttendanceSummaryDetailLevel(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail-level")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryDetailLevelHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryDetailLevelWidge))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail Widge By Level")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.SelectedPosition), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.IdClassroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.IdPeriod), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryDetailLevelWidgeResult))]
        public Task<IActionResult> GetAttendanceSummaryDetailLevelWidge(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail-level/widge")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryDetailLevelWidgeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryDetailLevelDownload))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail Download By Level")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.SelectedPosition), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.IdClassroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailLevelRequest.IdPeriod), In = ParameterLocation.Query)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> GetAttendanceSummaryDetailLevelDownload(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail-level/download")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryDetailLevelDownloadHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Attendance summary detail (Subject)
        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryDetailSubject))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail By Subject")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.SelectedPosition), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryDetailSubjectResult[]))]
        public Task<IActionResult> GetAttendanceSummaryDetailSubject(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail-subject")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            //var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryDetailSubjectHandler>();
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryDetailSubjectHandler2>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryDetailSubjectWidge))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail Widge By Subject")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.SelectedPosition), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryDetailLevelWidgeResult))]
        public Task<IActionResult> GetAttendanceSummaryDetailSubjectWidge(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail-subject/widge")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryDetailSubjectWidgeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryDetailSubjectDownload))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail Download By Subject")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.SelectedPosition), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSubjectRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> GetAttendanceSummaryDetailSubjectDownload(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail-subject/download")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryDetailSubjectDownloadHendler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Attendance summary detail (SchoolDay)
        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryDetailSchoolDay))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail By School Day")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.SelectedPosition), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.ClassId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.IdSession), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.IdClassroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryDetailSchoolDayResult[]))]
        public Task<IActionResult> GetAttendanceSummaryDetailSchoolDay(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail-school-day")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            //var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryDetailSchoolDayHandler>();
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryDetailSchoolDayHandler2>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryDayClassIdAndSession))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail By School Day")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDayClassIdAndSessionRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDayClassIdAndSessionRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDayClassIdAndSessionRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDayClassIdAndSessionRequest.SelectedPosition), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDayClassIdAndSessionRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDayClassIdAndSessionRequest.IdClassroom), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryDayClassIdAndSessionResult[]))]
        public Task<IActionResult> GetAttendanceSummaryDayClassIdAndSession(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail-school-day/class-id-and-session")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryDayClassIdAndSessionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryDetailSchoolDayWidge))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail Widge By Subject")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.SelectedPosition), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.IdClassroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.ClassId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.IdSession), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailSchoolDayRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryDetailSchoolDayWidgeResult))]
        public Task<IActionResult> GetAttendanceSummaryDetailSchoolDayWidge(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail-school-day/widge")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryDetailSchoolDayWidgeHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Dashboard
        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryDashboard))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Attendance Dashboard")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDashboardRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDashboardRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDashboardRequest.PeriodType), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryDashboardResult))]
        public Task<IActionResult> GetAttendanceSummaryDashboard(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/dashboard")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryDashboardHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryDashboardDetailParent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail Unexcused Excused By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDashboardDetailParentRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDashboardDetailParentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDashboardDetailParentRequest.PeriodType), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDashboardDetailParentRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDashboardDetailParentRequest.IdAttendance), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDashboardDetailParentRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDashboardDetailParentRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAttendanceSummaryDashboardDetailParentRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAttendanceSummaryDashboardDetailParentRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDashboardDetailParentRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryDetailUnexcusedExcusedResult))]
        public Task<IActionResult> GetAttendanceSummaryDashboardDetailParent(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/dashboard-parent")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryDashboardDetailParentHandler2>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region data inject
        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetInjectData))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Attendance Inject")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDetailRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> GetInjectData(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/inject")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetInjectDataHandler>();
            return handler.Execute(req, cancellationToken);
        }

        #endregion

        #region Email
        [FunctionName(nameof(AttendanceSummaryTermEndpoint.SendAttendanceSumamryEmail))]
        [OpenApiOperation(tags: _tag, Description = @"Send Attendance Summary Term Email")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(SendAttendanceSumamryEmailRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(SendAttendanceSumamryEmailRequest.Link), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(SendAttendanceSumamryEmailRequest.IdScenario), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(SendAttendanceSumamryEmailRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SendAttendanceSumamryEmail(
            [HttpTrigger(AuthorizationLevel.Function, "Get", Route = _route + "/email")] HttpRequest req,
              [Queue("notification-ehn")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SendAttendanceSumamryEmailHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }
        #endregion

        #region json validasi data
        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummarySubmitedJson))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Unsubmited")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.SelectedPosition), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdClassroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.StartDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.EndDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.ClassId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.SearchBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.GetAll), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(string))]
        public Task<IActionResult> GetAttendanceSummarySubmitedJson(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/submited-json")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummarySubmitedHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryUnsubmitedJson))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Unsubmited")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.SelectedPosition), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdClassroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.StartDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.EndDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceSummaryUnsubmitedRequest.ClassId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.SearchBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.GetAll), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(string))]
        public Task<IActionResult> GetAttendanceSummaryUnsubmitedJson(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/unsubmited-json")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryUnsubmitedJsonHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Score Summary
        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetStudentAttendanceSummaryTerm))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Attendance Summary Term")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetStudentAttendanceSummaryTermRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentAttendanceSummaryTermResult>))]
        public Task<IActionResult> GetStudentAttendanceSummaryTerm(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/student-detail")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentAttendanceSummaryTermHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetStudentAttendanceSummaryAllTerm))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Attendance Summary All Term",
             Description = @"
            - Smt filled : get all semester
            - Smt & Term filled : get spesifik until term")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentAttendanceSummaryAllTermRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentAttendanceSummaryAllTermRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentAttendanceSummaryAllTermRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentAttendanceSummaryAllTermRequest.Term), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentAttendanceSummaryAllTermResult>))]
        public Task<IActionResult> GetStudentAttendanceSummaryAllTerm(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student-allterm-detail")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentAttendanceSummaryAllTermHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetStudentAttendanceRateForScoreSummary))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail Attendance Rate By Student For Score Summary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentAttendanceRateForScoreSummaryRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentAttendanceRateForScoreSummaryRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentAttendanceRateForScoreSummaryRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentAttendanceRateForScoreSummaryRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentAttendanceRateForScoreSummaryRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentAttendanceRateForScoreSummaryRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentAttendanceRateForScoreSummaryResult>))]
        public Task<IActionResult> GetStudentAttendanceRateForScoreSummary(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student-attendance-rate-score-summary")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentAttendanceRateForScoreSummaryHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        [FunctionName(nameof(AttendanceSummaryTermEndpoint.GetAttendanceSummaryBySchoolDay))]
        [OpenApiOperation(tags: _tag, Summary = "Get Attendance Summary By School Day")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryBySchoolDayRequest.IdScheduleLesson), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryBySchoolDayRequest.Status), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetAttendanceSummaryBySchoolDayResult>))]
        public Task<IActionResult> GetAttendanceSummaryBySchoolDay(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student-By-School-Day")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceSummaryBySchoolDayHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
