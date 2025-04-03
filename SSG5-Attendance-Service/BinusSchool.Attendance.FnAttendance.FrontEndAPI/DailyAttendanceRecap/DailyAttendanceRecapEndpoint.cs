using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using BinusSchool.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using BinusSchool.Data.Model.Attendance.FnAttendance.DailyAttendanceRecap;
using System.ComponentModel.DataAnnotations;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance;

namespace BinusSchool.Attendance.FnAttendance.DailyAttendanceRecap
{
    public class DailyAttendanceRecapEndpoint
    {
        private const string _route = "daily-attendance-recap";
        private const string _tag = "Daily Attendance Recap";

        [FunctionName(nameof(DailyAttendanceRecapEndpoint.GetDailyAttendanceRecap))]
        [OpenApiOperation(tags: _tag, Summary = "Get Daily Attendance Recap")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDailyAttendanceRecapRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDailyAttendanceRecapRequest.IdBinusian), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDailyAttendanceRecapRequest.Semester), In = ParameterLocation.Query, Type = typeof(int?))]
        [OpenApiParameter(nameof(GetDailyAttendanceRecapRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDailyAttendanceRecapRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDailyAttendanceRecapRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDailyAttendanceRecapResult[]))]
        public Task<IActionResult> GetDailyAttendanceRecap(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
    CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDailyAttendanceRecapHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DailyAttendanceRecapEndpoint.GenerateExcelDailyAttendanceRecap))]
        [OpenApiOperation(tags: _tag, Summary = "Generate Excel Daily Attendance Recap")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GenerateExcelDailyAttendanceRecapRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GenerateExcelDailyAttendanceRecapResult))]
        public Task<IActionResult> GenerateExcelDailyAttendanceRecap(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/download")] HttpRequest req,
    CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GenerateExcelDailyAttendanceRecapHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
