using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;


namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary
{
    public class AttendanceSummaryEndpoint
    {
        private const string _route = "attendance-summary";
        private const string _tag = "Attendance Summary";

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryByRange))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary By Range")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryByRangeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryByRangeRequest.StartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryByRangeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryByRangeRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryByRangeRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SummaryResult))]
        public Task<IActionResult> GetSummaryByRange(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/by-range")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryByRange2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryByRange2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary By Range")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryByRangeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryByRangeRequest.StartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryByRangeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryByRangeRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryByRangeRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SummaryResult))]
        public Task<IActionResult> GetSummaryByRange2(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/by-range2")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryByRange2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryByPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary By Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryByPeriodRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryByPeriodRequest.Semester), In = ParameterLocation.Query, Required = true, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSummaryByPeriodRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryByPeriodRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryByPeriodRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SummaryResult))]
        public Task<IActionResult> GetSummaryByPeriod(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/by-period")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryByPeriod2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryByPeriod2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary By Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryByPeriodRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryByPeriodRequest.Semester), In = ParameterLocation.Query, Required = true, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSummaryByPeriodRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryByPeriodRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryByPeriodRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SummaryResult))]
        public Task<IActionResult> GetSummaryByPeriod2(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/by-period2")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryByPeriod2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryDetailByStudentByRange))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail By Student (by range)")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.StartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SummaryByStudentResult))]
        public Task<IActionResult> GetSummaryDetailByStudentByRange(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/by-student/by-range")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryDetailByStudentByRangeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryDetailByStudentByPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail By Student (by period)")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.Semester), In = ParameterLocation.Query, Required = true, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SummaryByStudentResult))]
        public Task<IActionResult> GetSummaryDetailByStudentByPeriod(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/by-student/by-period")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryDetailByStudentByPeriodHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryDetailBySchoolDayByRange))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail By School Day (by range)")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.StartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.ClassId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdSession), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SummaryBySchoolDayResult))]
        public Task<IActionResult> GetSummaryDetailBySchoolDayByRange(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/by-school-day/by-range")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryDetailBySchoolDayByRangeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryDetailBySchoolDayByPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail By School Day (by period)")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.Semester), In = ParameterLocation.Query, Required = true, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.ClassId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdSession), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SummaryBySchoolDayResult))]
        public Task<IActionResult> GetSummaryDetailBySchoolDayByPeriod(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/by-school-day/by-period")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryDetailBySchoolDayByPeriodHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryDetailByLevelByRange))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail By Level (by range)")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.StartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SummaryByLevelResult))]
        public Task<IActionResult> GetSummaryDetailByLevelByRange(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/by-level/by-range")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryDetailByLevelByRangeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryDetailByLevelByRangeTermDay))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail By Level (by range term day)")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.StartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SummaryByLevelResult))]
        public Task<IActionResult> GetSummaryDetailByLevelByRangeTermDay(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/by-level/by-range/term-day")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryDetailByLevelByRangeTermDayHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryDetailByLevelByPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail By Level (by period)")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.Semester), In = ParameterLocation.Query, Required = true, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SummaryByLevelResult))]
        public Task<IActionResult> GetSummaryDetailByLevelByPeriod(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/by-level/by-period")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryDetailByLevelByPeriodHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryDetailByLevelByPeriodTermDay))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail By Level (by period)")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.Semester), In = ParameterLocation.Query, Required = true, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SummaryByLevelResult))]
        public Task<IActionResult> GetSummaryDetailByLevelByPeriodTermDay(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/by-level/by-period/term-day")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryDetailByLevelByPeriodTermDayHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryDetailBySubjectByRange))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail By Subject (by range)")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.StartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SummaryBySubjectResult))]
        public Task<IActionResult> GetSummaryDetailBySubjectByRange(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/by-subject/by-range")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryDetailBySubjectByRangeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryDetailBySubjectByPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail By Subject (by period)")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.Semester), In = ParameterLocation.Query, Required = true, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SummaryBySubjectResult))]
        public Task<IActionResult> GetSummaryDetailBySubjectByPeriod(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/by-subject/by-period")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryDetailBySubjectByPeriodHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryDetailWidgetByRange))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail Widget (by range)")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.StartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByRangeRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SummaryDetailWidgetResult))]
        public Task<IActionResult> GetSummaryDetailWidgetByRange(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/widget/by-range")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryDetailWidgetByRangeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryDetailWidgetByPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Detail Widget (by period)")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.Semester), In = ParameterLocation.Query, Required = true, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailByPeriodRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SummaryDetailWidgetResult))]
        public Task<IActionResult> GetSummaryDetailWidgetByPeriod(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/widget/by-period")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryDetailWidgetByPeriodHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetAttendanceRateByStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Attendance Rate By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceRateByStudentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        //[OpenApiParameter(nameof(GetAttendanceRateByStudentRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceRateByStudentRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceRateByStudentRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceRateByStudentRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetAttendanceRateByStudentRequest.StartDate), In = ParameterLocation.Query, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetAttendanceRateByStudentRequest.EndDate), In = ParameterLocation.Query, Type = typeof(DateTime))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ICollection<GetAttendanceRateByStudentResult>))]

        public Task<IActionResult> GetAttendanceRateByStudent(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail-attendance-rate-by-student")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceRateByStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetAttendanceRateByStudentTermDay))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Attendance Rate By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceRateByStudentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        //[OpenApiParameter(nameof(GetAttendanceRateByStudentRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceRateByStudentRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceRateByStudentRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceRateByStudentRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetAttendanceRateByStudentRequest.StartDate), In = ParameterLocation.Query, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetAttendanceRateByStudentRequest.EndDate), In = ParameterLocation.Query, Type = typeof(DateTime))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceRateByStudentTermDayResult))]

        public Task<IActionResult> GetAttendanceRateByStudentTermDay(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail-attendance-rate-by-student-term-day")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceRateByStudentTermDayHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetDetailExcusedAbsentStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Excused Absent By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailExcusedAbsentStudentRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailExcusedAbsentStudentRequest.StartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetDetailExcusedAbsentStudentRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetDetailExcusedAbsentStudentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailExcusedAbsentStudentRequest.ExcusedAbsenceCategory), In = ParameterLocation.Query, Required = true, Type = typeof(ExcusedAbsenceCategory))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ICollection<GetDetailExcusedAbsentStudentResult>))]

        public Task<IActionResult> GetDetailExcusedAbsentStudent(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail-excused-absent")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailExcusedAbsentStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetDetailExcusedAbsentStudentTermDay))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Excused Absent By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailExcusedAbsentStudentRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailExcusedAbsentStudentRequest.StartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetDetailExcusedAbsentStudentRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetDetailExcusedAbsentStudentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailExcusedAbsentStudentRequest.ExcusedAbsenceCategory), In = ParameterLocation.Query, Required = true, Type = typeof(ExcusedAbsenceCategory))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ICollection<GetDetailExcusedAbsentStudentResult>))]

        public Task<IActionResult> GetDetailExcusedAbsentStudentTermDay(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail-excused-absent-term-day")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailExcusedAbsentStudentTermDayHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetDetailExcusedAbsentStudentAndPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Excused Absent By Student And Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailExcusedAbsentStudentAndPeriodRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailExcusedAbsentStudentAndPeriodRequest.Semester), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetDetailExcusedAbsentStudentAndPeriodRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailExcusedAbsentStudentAndPeriodRequest.ExcusedAbsenceCategory), In = ParameterLocation.Query, Required = true, Type = typeof(ExcusedAbsenceCategory))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ICollection<GetDetailExcusedAbsentStudentResult>))]

        public Task<IActionResult> GetDetailExcusedAbsentStudentAndPeriod(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail-excused-absent-and-period")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailExcusedAbsentStudentAndPeriodHandler>();
            return handler.Execute(req, cancellationToken);
        }
        [FunctionName(nameof(AttendanceSummaryEndpoint.GetDetailExcusedAbsentStudentAndPeriodTermDay))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Excused Absent By Student And Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailExcusedAbsentStudentAndPeriodRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailExcusedAbsentStudentAndPeriodRequest.Semester), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetDetailExcusedAbsentStudentAndPeriodRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailExcusedAbsentStudentAndPeriodRequest.ExcusedAbsenceCategory), In = ParameterLocation.Query, Required = true, Type = typeof(ExcusedAbsenceCategory))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ICollection<GetDetailExcusedAbsentStudentResult>))]

        public Task<IActionResult> GetDetailExcusedAbsentStudentAndPeriodTermDay(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail-excused-absent-and-period-term-day")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailExcusedAbsentStudentAndPeriodTermDayHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetDetailAttendanceToDateStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Absence To Date By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailAttendanceToDateByStudentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceToDateByStudentRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceToDateByStudentRequest.StartDate), In = ParameterLocation.Query, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetDetailAttendanceToDateByStudentRequest.EndDate), In = ParameterLocation.Query, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetDetailAttendanceToDateByStudentRequest.Semester), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ICollection<GetDetailAttendanceToDateByStudentResult>))]

        public Task<IActionResult> GetDetailAttendanceToDateStudent(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail-attendance-date-by-student")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailAttendanceToDateByStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetDetailWorkhabitStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Workhabit By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailAttendanceWorkhabitByStudentRequest.StartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetDetailAttendanceWorkhabitByStudentRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetDetailAttendanceWorkhabitByStudentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceWorkhabitByStudentRequest.IdMappingAttendanceWorkhabit), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ICollection<GetDetailAttendanceWorkhabitByStudentResult>))]

        public Task<IActionResult> GetDetailWorkhabitStudent(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail-workhabit-by-student")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailAttendanceWorkhabitByStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetDetailWorkhabitStudentTermDay))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Workhabit By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailAttendanceWorkhabitByStudentRequest.StartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetDetailAttendanceWorkhabitByStudentRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetDetailAttendanceWorkhabitByStudentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceWorkhabitByStudentRequest.IdMappingAttendanceWorkhabit), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ICollection<GetDetailAttendanceWorkhabitByStudentResult>))]

        public Task<IActionResult> GetDetailWorkhabitStudentTermDay(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail-workhabit-by-student-term-day")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailAttendanceWorkhabitByStudentTermDayHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetDetailWorkhabitStudentAndPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Workhabit By Student And Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailAttendanceWorkhabitByStudentAndPeriodRequest.Semester), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetDetailAttendanceWorkhabitByStudentAndPeriodRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceWorkhabitByStudentAndPeriodRequest.IdMappingAttendanceWorkhabit), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ICollection<GetDetailAttendanceWorkhabitByStudentAndPeriodRequest>))]

        public Task<IActionResult> GetDetailWorkhabitStudentAndPeriod(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail-workhabit-by-student-and-period")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailAttendanceWorkhabitByStudentAndPeriodHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetDetailWorkhabitStudentAndPeriodTermDay))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Workhabit By Student And Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailAttendanceWorkhabitByStudentAndPeriodRequest.Semester), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetDetailAttendanceWorkhabitByStudentAndPeriodRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailAttendanceWorkhabitByStudentAndPeriodRequest.IdMappingAttendanceWorkhabit), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ICollection<GetDetailAttendanceWorkhabitByStudentAndPeriodRequest>))]

        public Task<IActionResult> GetDetailWorkhabitStudentAndPeriodTermDay(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail-workhabit-by-student-and-period-term-day")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailAttendanceWorkhabitByStudentAndPeriodTermDayHandler>();
            return handler.Execute(req, cancellationToken);
        }



        [FunctionName(nameof(AttendanceSummaryEndpoint.GetAttendanceAndWorkhabitByLevel))]
        [OpenApiOperation(tags: _tag, Summary = "Get Attendance & Workhabit By Level")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceAndWorkhabitByLevelRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ICollection<GetDetailAttendanceWorkhabitByStudentResult>))]

        public Task<IActionResult> GetAttendanceAndWorkhabitByLevel(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-attendance-and-workhabit-by-level")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceAndWorkhabitByLevelHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetOverallAttendanceRateByStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Overall Attendance Rate By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetOverallAttendanceRateByStudentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetOverallAttendanceRateByStudentRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetOverallAttendanceRateByStudentRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ICollection<GetOverallAttendanceRateByStudentResult>))]

        public Task<IActionResult> GetOverallAttendanceRateByStudent(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-overall-attendance-rate-by-student")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetOverallAttendanceRateByStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetOverallTermAttendanceRateByStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Overall Term Attendance Rate By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetOverallTermAttendanceRateByStudentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetOverallTermAttendanceRateByStudentRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetOverallTermAttendanceRateByStudentRequest.Term), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ICollection<GetOverallTermAttendanceRateByStudentResult>))]

        public Task<IActionResult> GetOverallTermAttendanceRateByStudent(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-overall-term-attendance-rate-by-student")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetOverallTermAttendanceRateByStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetAvailabilityPositionByUser))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAvailabilityPositionByUserRequest.IdAcademicyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAvailabilityPositionByUserRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAvailabilityPositionByUserResult))]
        public Task<IActionResult> GetAvailabilityPositionByUser(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-avaiability-position")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAvailabilityPositionByUserHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryUnsubmittedByRange))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Unsubmitted By Range")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.StartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.Page), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.Size), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<UnresolvedAttendanceGroupResult>))]
        public Task<IActionResult> GetSummaryUnsubmittedByRange(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-unsubmitted-by-range")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryUnsubmittedByRangeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryUnsubmittedByRangeTermDay))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Unsibmitted By Range Term Day")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.StartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.Page), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.Size), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<UnresolvedAttendanceTermDayResult>))]
        public Task<IActionResult> GetSummaryUnsubmittedByRangeTermDay(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-unsubmitted-by-range-term-day")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryUnsubmittedByRangeTermDayHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryByUnsubmittedPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Unsubmitted By Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.Semester), In = ParameterLocation.Query, Required = true, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.Page), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.Size), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<UnresolvedAttendanceGroupResult>))]
        public Task<IActionResult> GetSummaryByUnsubmittedPeriod(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-unsubmitted-by-period")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryUnsubmittedByPeriodHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryByUnsubmittedPeriodTermDay))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Unsubmitted By Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.Semester), In = ParameterLocation.Query, Required = true, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.Page), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.Size), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<UnresolvedAttendanceTermDayResult>))]
        public Task<IActionResult> GetSummaryByUnsubmittedPeriodTermDay(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-unsubmitted-by-period-term-day")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryUnsubmittedByPeriodTermDayHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #region Summary Pending By Range Term Day And Session
        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryPendingByRange))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Pending By Range")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.StartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.Page), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.Size), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<UnresolvedAttendanceGroupResult>))]
        public Task<IActionResult> GetSummaryPendingByRange(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-pending-by-range")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryPendingByRangeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryPendingByRangeTermDay))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Pending By Range Term Day")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.StartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.Page), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.Size), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<UnresolvedAttendanceTermDayResult>))]
        public Task<IActionResult> GetSummaryPendingByRangeTermDay(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-pending-by-range-term-day")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryPendingByRangeTermDayHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Summary pending By Period Term Day And Session

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryByPendingPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Pending By Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.Semester), In = ParameterLocation.Query, Required = true, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.Page), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.Size), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<UnresolvedAttendanceGroupResult>))]
        public Task<IActionResult> GetSummaryByPendingPeriod(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-pending-by-period")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryPendingByPeriodHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryByPendingPeriodTermDay))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Unsubmitted By Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.Semester), In = ParameterLocation.Query, Required = true, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByPeriodRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.Page), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryUnsubmittedByRangeRequest.Size), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<UnresolvedAttendanceTermDayResult>))]
        public Task<IActionResult> GetSummaryByPendingPeriodTermDay(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-pending-by-period-term-day")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryPendingByPeriodTermDayHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Summary Detail Unsubmitted And Pending By Level Term Day And Session
        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryDetailUnsubmittedByLevelByPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Unsubmitted By Level By Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByPeriodRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByPeriodRequest.Size), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByPeriodRequest.Page), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetSummaryDetailUnsubmittedByLevelByPeriodResponse>))]
        public Task<IActionResult> GetSummaryDetailUnsubmittedByLevelByPeriod(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/unsubmitted/by-level/by-period")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryDetailUnsubmittedByLevelByPeriodHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryDetailUnsubmittedByLevelByPeriodTermDay))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Unsubmitted By Level By Period Term Day")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByPeriodRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByPeriodRequest.Size), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByPeriodRequest.Page), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetSummaryDetailUnsubmittedByLevelByPeriodTermDayResponse>))]
        public Task<IActionResult> GetSummaryDetailUnsubmittedByLevelByPeriodTermDay(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/unsubmitted/by-level/by-period/term-day")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryDetailUnsubmittedByLevelByPeriodTermDayHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryDetailUnsubmittedByLevelByRange))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Unsubmitted By Level By Range")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.Size), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.Page), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetSummaryDetailUnsubmittedByLevelByPeriodResponse>))]
        public Task<IActionResult> GetSummaryDetailUnsubmittedByLevelByRange(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/unsubmitted/by-level/by-range")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryDetailUnsubmittedByLevelByRangeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryDetailUnsubmittedByLevelByRangeTermDay))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Unsubmitted By Level By Range Term Day")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.Size), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.Page), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetSummaryDetailUnsubmittedByLevelByPeriodTermDayResponse>))]
        public Task<IActionResult> GetSummaryDetailUnsubmittedByLevelByRangeTermDay(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/unsubmitted/by-level/by-range/term-day")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryDetailUnsubmittedByLevelByRangeTermDayHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryDetailPendingByLevelByPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Pending By Level By Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByPeriodRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByPeriodRequest.Size), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByPeriodRequest.Page), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetSummaryDetailUnsubmittedByLevelByPeriodResponse>))]
        public Task<IActionResult> GetSummaryDetailPendingByLevelByPeriod(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/pending/by-level/by-period")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryDetailPendingByLevelByPeriodHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryDetailPendingByLevelByPeriodTermDay))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Pending By Level By Period Term Day")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByPeriodRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByPeriodRequest.Size), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByPeriodRequest.Page), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetSummaryDetailUnsubmittedByLevelByPeriodTermDayResponse>))]
        public Task<IActionResult> GetSummaryDetailPendingByLevelByPeriodTermDay(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/pending/by-level/by-period/term-day")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryDetailPendingByLevelByPeriodTermDayHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryDetailPendingByLevelByRange))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Pending By Level By Range")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.Size), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.Page), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetSummaryDetailUnsubmittedByLevelByPeriodResponse>))]
        public Task<IActionResult> GetSummaryDetailPendingByLevelByRange(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/pending/by-level/by-range")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryDetailPendingByLevelByRangeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetSummaryDetailPendingByLevelByRangeTermDay))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Pending By Level By Range Term Day")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.Size), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.Page), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetSummaryDetailUnsubmittedByLevelByPeriodTermDayResponse>))]
        public Task<IActionResult> GetSummaryDetailPendingByLevelByRangeTermDay(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/pending/by-level/by-range/term-day")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryDetailPendingByLevelByRangeTermDayHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Summary for dashboard
        [FunctionName(nameof(AttendanceSummaryEndpoint.GetStudentAttendanceSummary))]
        [OpenApiOperation(tags: _tag, Summary = "Get Attendance Summary for Specific Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentAttendanceSummaryRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentAttendanceSummaryRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentAttendanceSummaryResult))]
        public Task<IActionResult> GetStudentAttendanceSummary(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentAttendanceSummaryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetStudentAttendanceSummaryDayDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Attendance Summary detail for Specific Student on Term Day")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentAttendanceSummaryDetailRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentAttendanceSummaryDetailRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentAttendanceSummaryDetailRequest.IdAttendance), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentAttendanceSummaryDetailRequest.PeriodType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentAttendanceSummaryDetailRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentAttendanceSummaryDetailRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentAttendanceSummaryDetailRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentAttendanceSummaryDetailRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentAttendanceSummaryDayDetailResult>))]
        public Task<IActionResult> GetStudentAttendanceSummaryDayDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student/day")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentAttendanceSummaryDayDetailHandler>();
            return handler.Execute(req, cancellationToken);
        }
        [FunctionName(nameof(AttendanceSummaryEndpoint.GetStudentAttendanceSummarySessionDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Attendance Summary detail for Specific Student on Term Session")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentAttendanceSummaryDetailRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentAttendanceSummaryDetailRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentAttendanceSummaryDetailRequest.IdAttendance), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentAttendanceSummaryDetailRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentAttendanceSummaryDetailRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentAttendanceSummaryDetailRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentAttendanceSummaryDetailRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentAttendanceSummarySessionDetailResult>))]
        public Task<IActionResult> GetStudentAttendanceSummarySessionDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student/session")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentAttendanceSummarySessionDetailHandler>();
            return handler.Execute(req, cancellationToken);
        }
        [FunctionName(nameof(AttendanceSummaryEndpoint.GetStudentWorkhabitDayDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Workhabit detail for Specific Student on Term Day")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentWorkhabitDetailRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentWorkhabitDetailRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentWorkhabitDetailRequest.IdWorkhabit), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentWorkhabitDetailRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentWorkhabitDetailRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentWorkhabitDetailRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentWorkhabitDetailRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentWorkhabitDayDetailResult>))]
        public Task<IActionResult> GetStudentWorkhabitDayDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student/workhabit/day")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentWorkhabitDayDetailHandler>();
            return handler.Execute(req, cancellationToken);
        }
        [FunctionName(nameof(AttendanceSummaryEndpoint.GetStudentWorkhabitSessionDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get AWorkhabit detail for Specific Student on Term Session")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentWorkhabitDetailRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentWorkhabitDetailRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentWorkhabitDetailRequest.IdWorkhabit), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentWorkhabitDetailRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentWorkhabitDetailRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentWorkhabitDetailRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentWorkhabitDetailRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentWorkhabitSessionDetailResult>))]
        public Task<IActionResult> GetStudentWorkhabitSessionDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student/workhabit/session")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentWorkhabitSessionDetailHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Download All Unexcused 
        [FunctionName(nameof(AttendanceSummaryEndpoint.GetDownloadAllUnexcusedAbsenceByRange))]
        [OpenApiOperation(tags: _tag, Summary = "Get Download All Unexcused Absence By Range")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByRangeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByRangeRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByRangeRequest.StartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByRangeRequest.EndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByRangeRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByRangeRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByRangeRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByRangeRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByRangeRequest.IsDailyAttendance), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SummaryByStudentResult))]
        public Task<IActionResult> GetDownloadAllUnexcusedAbsenceByRange(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-download-all-unexcused-absence-by-range")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDownloadAllUnexcusedAbsenceByRangeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryEndpoint.GetDownloadAllUnexcusedAbsenceByPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Get Download All Unexcused Absence By Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByPeriodRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByPeriodRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByPeriodRequest.Semester), In = ParameterLocation.Query, Required = true, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByPeriodRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByPeriodRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByPeriodRequest.ClassId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByPeriodRequest.IdSession), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByPeriodRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByPeriodRequest.SelectedPosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetDownloadAllUnexcusedAbsenceByPeriodRequest.IsDailyAttendance), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDownloadAllUnexcusedAbsenceByPeriodResult))]
        public Task<IActionResult> GetDownloadAllUnexcusedAbsenceByPeriod(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-download-all-unexcused-absence-by-period")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDownloadAllUnexcusedAbsenceByPeriodHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

    }
}
