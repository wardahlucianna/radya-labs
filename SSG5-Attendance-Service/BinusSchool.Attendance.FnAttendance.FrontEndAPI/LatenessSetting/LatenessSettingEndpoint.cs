using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using BinusSchool.Attendance.FnAttendance.Formula;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Attendance.FnAttendance.Formula;
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
using BinusSchool.Data.Model.Attendance.FnAttendance.LatenessSetting;
using BinusSchool.Attendance.FnAttendance.AttendanceEntry;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceEntry;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace BinusSchool.Attendance.FnAttendance.LatenessSetting
{
    public class LatenessSettingEndpoint
    {
        private const string _route = "lateness-setting";
        private const string _tag = "Lateness Setting";

        [FunctionName(nameof(LatenessSettingEndpoint.GetLatenessSettingDetail))]
        [OpenApiOperation(tags: _tag, Description = "Detail Lateness Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("idLevel", In = ParameterLocation.Path, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLatenessSettingDetailResult))]
        public Task<IActionResult> GetLatenessSettingDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetLatenessSettingDetailHandler>();
            return handler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(LatenessSettingEndpoint.AddAndUpdateLatenessSetting))]
        [OpenApiOperation(tags: _tag, Description = "add and update Lateness Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddAndUpdateLatenessSettingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddAndUpdateLatenessSetting(
             [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
             CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetLatenessSettingDetailHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
