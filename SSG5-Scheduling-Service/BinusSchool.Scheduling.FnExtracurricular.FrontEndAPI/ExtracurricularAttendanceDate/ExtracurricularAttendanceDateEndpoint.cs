using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendanceDate;
using BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendance;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendanceDate
{
    public class ExtracurricularAttendanceDateEndpoint
    {
        private const string _route = "extracurricular-attendance-date";
        private const string _tag = "Extracurricular Attendance Date";

        private readonly GetElectiveAttendanceByDateHandler _getElectiveAttendanceByDateHandler;

        public ExtracurricularAttendanceDateEndpoint(GetElectiveAttendanceByDateHandler getElectiveAttendanceByDateHandler)
        {
            _getElectiveAttendanceByDateHandler = getElectiveAttendanceByDateHandler;
        }


        [FunctionName(nameof(ExtracurricularAttendanceDateEndpoint.GetElectiveAttendanceByDate))]
        [OpenApiOperation(tags: _tag, Summary = "Get Extracurricular Attendance by Date")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetElectiveAttendanceByDateRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetElectiveAttendanceByDateRequest.Date), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetElectiveAttendanceByDateRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetElectiveAttendanceByDateResult>))]
        public Task<IActionResult> GetElectiveAttendanceByDate(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-extracurricular-attendance-bydate")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getElectiveAttendanceByDateHandler.Execute(req, cancellationToken);
        }
    }
}
