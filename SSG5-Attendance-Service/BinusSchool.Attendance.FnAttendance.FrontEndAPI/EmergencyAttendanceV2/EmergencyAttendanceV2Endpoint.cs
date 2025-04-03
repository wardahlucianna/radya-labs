using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using BinusSchool.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs;
using Microsoft.OpenApi.Models;
using System.Threading.Tasks;
using System.Threading;
using BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Document.FnDocument.BLPGroup;

namespace BinusSchool.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class EmergencyAttendanceV2Endpoint
    {
        private const string _route = "emergency-attendance-v2";
        private const string _tag = "Emergency Attendance v2";
        public GetStudentEmergencyAttendanceHandler _studentEmergencyAttendanceHandler;
        public GetEmergencyAttendanceSummaryHandler _getEmergencyAttendanceSummaryHandler;
        public SaveStudentEmergencyAttendanceHandler _saveEmergencyAttendanceReportHandler;
        public GetEmergencyAttendancePrivilegeHandler _getEmergencyAttendancePrivilegeHandler;
        public UpdateEmergencyAttendanceReportHandler _updateEmergencyAttendanceReportHandler;
        public GetEmergencyReportListHandler _getEmergencyReportListHandler;
        public GetEmergencyReportDetailHandler _getEmergencyReportDetailHandler;
        public ExportExcelEmergencyReportDetailHandler _exportExcelEmergencyReportDetailHandler;

        public EmergencyAttendanceV2Endpoint(GetStudentEmergencyAttendanceHandler studentEmergencyAttendanceHandler,
            GetEmergencyAttendanceSummaryHandler getEmergencyAttendanceSummaryHandler,
            SaveStudentEmergencyAttendanceHandler saveEmergencyAttendanceReportHandler,
            GetEmergencyAttendancePrivilegeHandler getEmergencyAttendancePrivilegeHandler,
            UpdateEmergencyAttendanceReportHandler updateEmergencyAttendanceReportHandler,
            GetEmergencyReportListHandler getEmergencyReportListHandler,
            GetEmergencyReportDetailHandler getEmergencyReportDetailHandler,
            ExportExcelEmergencyReportDetailHandler exportExcelEmergencyReportDetailHandler)
        {
            _studentEmergencyAttendanceHandler = studentEmergencyAttendanceHandler;
            _getEmergencyAttendanceSummaryHandler = getEmergencyAttendanceSummaryHandler;
            _saveEmergencyAttendanceReportHandler = saveEmergencyAttendanceReportHandler;
            _getEmergencyAttendancePrivilegeHandler = getEmergencyAttendancePrivilegeHandler;
            _updateEmergencyAttendanceReportHandler = updateEmergencyAttendanceReportHandler;
            _getEmergencyReportListHandler = getEmergencyReportListHandler;
            _getEmergencyReportDetailHandler = getEmergencyReportDetailHandler;
            _exportExcelEmergencyReportDetailHandler = exportExcelEmergencyReportDetailHandler;

        }


        [FunctionName(nameof(EmergencyAttendanceV2Endpoint.GetStudentEmergencyAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Emergency Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentEmergencyAttendanceRequest.Search), In = ParameterLocation.Query)]
        //[OpenApiParameter(nameof(GetStudentEmergencyAttendanceRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentEmergencyAttendanceRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentEmergencyAttendanceRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        //[OpenApiParameter(nameof(GetStudentEmergencyAttendanceRequest.OrderBy), In = ParameterLocation.Query)]
        //[OpenApiParameter(nameof(GetStudentEmergencyAttendanceRequest.OrderType), In = ParameterLocation.Query)]
        //[OpenApiParameter(nameof(GetStudentEmergencyAttendanceRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetStudentEmergencyAttendanceRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentEmergencyAttendanceRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentEmergencyAttendanceRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentEmergencyAttendanceRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentEmergencyAttendanceRequest.IdScheduleLesson), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentEmergencyAttendanceRequest.Status), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentEmergencyAttendanceRequest.IdEmergencyStatus), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentEmergencyAttendanceResult))]
        public Task<IActionResult> GetStudentEmergencyAttendance(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student-emergency-attendance")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _studentEmergencyAttendanceHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EmergencyAttendanceV2Endpoint.GetEmergencyAttendanceSummary))]
        [OpenApiOperation(tags: _tag, Summary = "Get Emergency Attendance Summary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetEmergencyAttendanceSummaryRequest.Date), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetEmergencyAttendanceSummaryRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEmergencyAttendanceSummaryRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetEmergencyAttendanceSummaryResult))]
        public Task<IActionResult> GetEmergencyAttendanceSummary(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-emergency-attendance-summary")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getEmergencyAttendanceSummaryHandler.Execute(req, cancellationToken);

        }

        [FunctionName(nameof(EmergencyAttendanceV2Endpoint.SaveStudentEmergencyAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Save Student Emergency Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveStudentEmergencyAttendanceRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SaveStudentEmergencyAttendanceResult))]
        public Task<IActionResult> SaveStudentEmergencyAttendance(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-student-emergency-attendace")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _saveEmergencyAttendanceReportHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EmergencyAttendanceV2Endpoint.GetEmergencyAttendancePrivilege))]
        [OpenApiOperation(tags: _tag, Summary = "Get Emergency Attendance Privilege")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetEmergencyAttendancePrivilegeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEmergencyAttendancePrivilegeRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetEmergencyAttendancePrivilegeResult))]
        public Task<IActionResult> GetEmergencyAttendancePrivilege(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-emergency-attendance-privilege")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getEmergencyAttendancePrivilegeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EmergencyAttendanceV2Endpoint.UpdateEmergencyAttendanceReport))]
        [OpenApiOperation(tags: _tag, Summary = "Update Emergency Attendance Report")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateEmergencyAttendanceReportRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(UpdateEmergencyAttendanceReportResult))]
        public Task<IActionResult> UpdateEmergencyAttendanceReport(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/update-emergency-attendace-report")] HttpRequest req,
         [Queue("sendemail-emergency-attendance-queue")] ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            return _updateEmergencyAttendanceReportHandler.Execute(req, cancellationToken, false, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(EmergencyAttendanceV2Endpoint.GetEmergencyReportList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Emergency Report List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetEmergencyReportListRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEmergencyReportListRequest.semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEmergencyReportListRequest.startDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEmergencyReportListRequest.endDate), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetEmergencyReportListResult>))]
        public Task<IActionResult> GetEmergencyReportList(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-emergency-report-list")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getEmergencyReportListHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(EmergencyAttendanceV2Endpoint.GetEmergencyReportDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Emergency Report Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetEmergencyReportDetailRequest.Search), In = ParameterLocation.Query)]
        //[OpenApiParameter(nameof(GetEmergencyReportDetailRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEmergencyReportDetailRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetEmergencyReportDetailRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetEmergencyReportDetailRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEmergencyReportDetailRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEmergencyReportDetailRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetEmergencyReportDetailRequest.idEmergencyReport), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEmergencyReportDetailRequest.idLevel), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetEmergencyReportDetailRequest.idGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEmergencyReportDetailRequest.idHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEmergencyReportDetailRequest.IdEmergencyStatus), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetEmergencyReportDetailResult>))]
        public Task<IActionResult> GetEmergencyReportDetail(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-emergency-report-detail")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _getEmergencyReportDetailHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(EmergencyAttendanceV2Endpoint.ExportExcelEmergencyReportDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Export Emergency Report Detail to Excel")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportExcelEmergencyReportDetailRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelEmergencyReportDetail([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/emergency-report-detail-excel")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _exportExcelEmergencyReportDetailHandler.Execute(req, cancellationToken, false);
        }
    }
}
