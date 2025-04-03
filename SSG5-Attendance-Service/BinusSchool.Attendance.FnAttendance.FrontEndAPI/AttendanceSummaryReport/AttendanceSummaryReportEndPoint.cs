using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryReport;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryReport
{
    public class AttendanceSummaryReportEndPoint
    {
        private const string _route = "attendance-summary-report";
        private const string _tag = "Attendance Summary Report";

        private readonly GetAttendanceSummaryDailyReportHandler _getAttendanceSummaryDailyReportHandler;
        private readonly ExportExcelAttandanceSummaryUAPresentDailyHandler _exportExcelAttandanceSummaryUAPresentDailyHandler;

        public AttendanceSummaryReportEndPoint(
            GetAttendanceSummaryDailyReportHandler getAttendanceSummaryDailyReportHandler,
            ExportExcelAttandanceSummaryUAPresentDailyHandler exportExcelAttandanceSummaryUAPresentDailyHandler
            )
        {
            _getAttendanceSummaryDailyReportHandler = getAttendanceSummaryDailyReportHandler;
            _exportExcelAttandanceSummaryUAPresentDailyHandler = exportExcelAttandanceSummaryUAPresentDailyHandler;
        }

        [FunctionName(nameof(AttendanceSummaryReportEndPoint.GetAttendanceSummaryDailyReport))]
        [OpenApiOperation(tags: _tag, Summary = "Get Attendance Summary Daily Report")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDailyReportRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDailyReportRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDailyReportRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceSummaryDailyReportRequest.AttendanceDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetAttendanceSummaryDailyReportRequest.Levels), In = ParameterLocation.Query, Required = true, Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceSummaryDailyReportResult))]
        public Task<IActionResult> GetAttendanceSummaryDailyReport(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/GetAttendanceSummaryDailyReport")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getAttendanceSummaryDailyReportHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceSummaryReportEndPoint.ExportExcelAttandanceSummaryUAPresentDaily))]
        [OpenApiOperation(tags: _tag, Summary = "Export Excel Attandance Summary UA Present Daily")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportExcelAttandanceSummaryUAPresentDailyRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelAttandanceSummaryUAPresentDaily(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/ExportExcelAttandanceSummaryUAPresentDaily")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _exportExcelAttandanceSummaryUAPresentDailyHandler.Execute(req, cancellationToken);
        }
    }
}
