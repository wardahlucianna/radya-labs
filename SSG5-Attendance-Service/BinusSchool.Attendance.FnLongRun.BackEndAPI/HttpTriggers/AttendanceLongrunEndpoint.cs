using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Data.Model.Attendance.FnAttendanceLongrun.Longrun;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Attendance.FnLongRun.HttpTriggers
{
    public class AttendanceLongrunEndpoint
    {
        private const string _route = "attendancelongrun";
        private const string _tag = "Attendance Longrun List";

        [FunctionName(nameof(RunAttendanceSummaryBySchoolAndAcademicYear))]
        [OpenApiOperation(tags: _tag, Summary = "Trigger manual by school and id academic year")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key",
            In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization",
            In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(RunManuallyRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(RunManuallyRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUnresolvedAttendanceResult))]
        public Task<IActionResult> RunAttendanceSummaryBySchoolAndAcademicYear(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)]
            HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler =
                req.HttpContext.RequestServices.GetService<RunAttendanceSummaryBySchoolAndAcademicYearHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
