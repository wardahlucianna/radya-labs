using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceEntry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Attendance.FnAttendance.AttendanceEntry
{
    public class AttendanceEntryEndpoint
    {
        private const string _route = "attendance-entry";
        private const string _tag = "Attendance Entry Detail";

        [FunctionName(nameof(AttendanceEntryEndpoint.GetAttendanceDetail))]
        [OpenApiOperation(tags: _tag, Description = @"
            - ClassId: ClassId from GET /attendance-fn-attendance/class-and-session
            - IdSession: Sessions.Id from GET /attendance-fn-attendance/class-and-session")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAttendanceEntryRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceEntryRequest.CurrentPosition), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceEntryRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAttendanceEntryRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAttendanceEntryRequest.Date), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(GetAttendanceEntryRequest.ClassId), In = ParameterLocation.Query, Description = "Fill if AbsentTerm is Session")]
        [OpenApiParameter(nameof(GetAttendanceEntryRequest.IdSession), In = ParameterLocation.Query, Description = "Fill if AbsentTerm is Session")]
        [OpenApiParameter(nameof(GetAttendanceEntryRequest.Status), In = ParameterLocation.Query, Description = "Pending|Submitted|Unsubmitted")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAttendanceEntryResult))]
        public Task<IActionResult> GetAttendanceDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAttendanceEntryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceEntryEndpoint.UpdateAttendanceDetail))]
        [OpenApiOperation(tags: _tag, Description = @"
            - Id: IdHomeroom from GET /attendance-fn-attendance/attendance-entry
            - ClassId: ClassId from GET /attendance-fn-attendance/class-and-session
            - IdSession: Sessions.Id from GET /attendance-fn-attendance/class-and-session
            - CurrentPosition: Current user assignment, with constant from BinusSchool.Common.Constants.PositionConstant (require when absent term: Session)
            - Entries.IdAttendanceMapAttendance: Attendances.IdAttendanceMapAttendance from GET /attendance-fn-attendance/detail/{idLevel}
            - Entries.File: Payload from POST /attendance-fn-attendance/attendance-entry/upload-file")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateAttendanceEntryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateAttendanceDetail(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
              [Queue("notification-atd-attendance")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateAttendanceEntryHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(AttendanceEntryEndpoint.UpdateAllAttendanceDetail))]
        [OpenApiOperation(tags: _tag, Description = @"
            - Id: IdHomeroom from GET /attendance-fn-attendance/attendance-entry
            - IdSession: Sessions.Id from GET /attendance-fn-attendance/class-and-session
            - CurrentPosition: Current user assignment, with constant from BinusSchool.Common.Constants.PositionConstant (require when absent term: Session)")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateAllAttendanceEntryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateAllAttendanceDetail(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/all-present")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateAllAttendanceEntryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceEntryEndpoint.UploadFileAttendanceEntry))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(UploadFileAttendanceEntryRequest.IdGeneratedScheduleLesson), In = ParameterLocation.Query, Required = true)]
        // [OpenApiRequestBody("multipart/form-data", typeof(IFormFile), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(string))]
        public Task<IActionResult> UploadFileAttendanceEntry(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/upload-file")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UploadFileAttendanceEntryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AttendanceEntryEndpoint.DownloadFileAttendanceEntry))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DownloadFileAttendanceEntryRequest.FileName), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DownloadFileAttendanceEntry(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/download-file")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DownloadFileAttendanceEntryHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
