using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendance
{
    public class ExtracurricularAttendanceEndpoint
    {
        private const string _route = "extracurricular-attendance";
        private const string _tag = "Extracurricular Attendance";

        private readonly GetActiveUnsubmittedAttendanceHandler _getActiveUnsubmittedAttendanceHandler;
        private readonly GetStudentAttendanceHandler _getStudentAttendanceHandler;
        private readonly GetAttendanceStatusBySchoolHandler _getAttendanceStatusBySchoolHandler;
        private readonly GetStudentAttendanceDetailHandler _getStudentAttendanceDetailHandler;
        private readonly ExportExcelActiveUnsubmittedAttendanceHandler _excelActiveUnsubmittedAttendanceHandler;
        public ExtracurricularAttendanceEndpoint(
            GetActiveUnsubmittedAttendanceHandler getActiveUnsubmittedAttendanceHandler,
            GetStudentAttendanceHandler getStudentAttendanceHandler,
            GetAttendanceStatusBySchoolHandler getAttendanceStatusBySchoolHandler,
            GetStudentAttendanceDetailHandler getStudentAttendanceDetailHandler,
            ExportExcelActiveUnsubmittedAttendanceHandler excelActiveUnsubmittedAttendanceHandler)
        {
            _getActiveUnsubmittedAttendanceHandler = getActiveUnsubmittedAttendanceHandler;
            _getStudentAttendanceHandler = getStudentAttendanceHandler;
            _getAttendanceStatusBySchoolHandler = getAttendanceStatusBySchoolHandler;
            _getStudentAttendanceDetailHandler = getStudentAttendanceDetailHandler;
            _excelActiveUnsubmittedAttendanceHandler = excelActiveUnsubmittedAttendanceHandler;
        }

        [FunctionName(nameof(ExtracurricularAttendanceEndpoint.GetActiveUnsubmittedAttendanceHandler))]
        [OpenApiOperation(tags: _tag, Summary = "Get Active Unsubmitted Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetActiveUnsubmittedAttendanceRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetActiveUnsubmittedAttendanceResult))]
        public Task<IActionResult> GetActiveUnsubmittedAttendanceHandler([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-active-unsubmitted-attendance")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getActiveUnsubmittedAttendanceHandler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(ExtracurricularAttendanceEndpoint.GetStudentAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentAttendanceRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentAttendanceRequest.Semester), In = ParameterLocation.Query, Required = true, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentAttendanceRequest.IdExtracurricular), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentAttendanceRequest.Month), In = ParameterLocation.Query, Required = true, Type = typeof(int))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentAttendanceResult))]
        public Task<IActionResult> GetStudentAttendance(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-student-attendance")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getStudentAttendanceHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExtracurricularAttendanceEndpoint.GetAttendanceStatusBySchool))]
        [OpenApiOperation(tags: _tag, Summary = "Get Attendance Status By School")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceStatusBySchoolRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceStatusBySchoolResult))]
        public Task<IActionResult> GetAttendanceStatusBySchool(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-attendance-status-by-school")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getAttendanceStatusBySchoolHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExtracurricularAttendanceEndpoint.GetStudentAttendanceDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Attendance Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentAttendanceDetailRequest.IdExtracurricular), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentAttendanceDetailRequest.IdExtracurricularGeneratedAtt), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentAttendanceDetailResult))]
        public Task<IActionResult> GetStudentAttendanceDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-student-attendance-detail")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getStudentAttendanceDetailHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExtracurricularAttendanceEndpoint.UpdateExtracurricularAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Update Extracurricular Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateExtracurricularAttendanceRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateExtracurricularAttendance(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/update-extracurricular-attendance")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateExtracurricularAttendanceHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExtracurricularAttendanceEndpoint.AddSessionExtracurricularAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Add Session Extracurricular Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddSessionExtracurricularAttendanceRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddSessionExtracurricularAttendance(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/add-session-extracurricular-attendance")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddSessionExtracurricularAttendanceHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExtracurricularAttendanceEndpoint.ExportExcelSummaryExtracurricularAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Export Excel Summary Extracurricular Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportExcelSummaryExtracurricularAttendanceRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelSummaryExtracurricularAttendance([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/export-excel-summary-extracurricular-attendance")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ExportExcelSummaryExtracurricularAttendanceHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(ExtracurricularAttendanceEndpoint.DeleteExtracurricularAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Session Extracurricular Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteSessionExtracurricularAttendanceRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteExtracurricularAttendance(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-session-extracurricular-attendance")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteExtracurricularAttendanceHandler>();
            return handler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(ExtracurricularAttendanceEndpoint.ExportExcelActiveUnsubmittedAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Export Excel Active Unsubmitted Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetActiveUnsubmittedAttendanceRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelActiveUnsubmittedAttendance(
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/export-excel-unsubmiited-attendance")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _excelActiveUnsubmittedAttendanceHandler.Execute(req, cancellationToken, false);
        }

    }
}
