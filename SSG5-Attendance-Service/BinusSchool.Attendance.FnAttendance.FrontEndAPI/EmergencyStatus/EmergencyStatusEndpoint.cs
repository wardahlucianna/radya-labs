using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using BinusSchool.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs;
using Microsoft.OpenApi.Models;
using System.Threading.Tasks;
using System.Threading;

namespace BinusSchool.Attendance.FnAttendance.EmergencyStatus
{
    public class EmergencyStatusEndpoint
    {
        private const string _route = "emergency-status";
        private const string _tag = "Emergency Status";

        private readonly EmergencyStatusHandler _emergencyStatusHandler;
        public EmergencyStatusEndpoint(EmergencyStatusHandler emergencyStatusHandler)
        {
            _emergencyStatusHandler = emergencyStatusHandler;
        }

        [FunctionName(nameof(EmergencyStatusEndpoint.GetEmergencyStatus))]
        [OpenApiOperation(tags: _tag, Summary = "Get Emergency Status List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<CodeWithIdVm>))]
        public Task<IActionResult> GetEmergencyStatus(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _emergencyStatusHandler.Execute(req, cancellationToken);
        }
    }
}
