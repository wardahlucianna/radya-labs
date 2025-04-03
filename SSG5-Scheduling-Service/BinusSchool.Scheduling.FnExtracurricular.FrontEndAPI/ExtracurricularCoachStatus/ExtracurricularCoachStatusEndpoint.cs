using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularCoachStatus
{
    public class ExtracurricularCoachStatusEndpoint
    {
        private const string _route = "elective/coach-status";
        private const string _tag = "Elective Coach Status";

        private readonly ExtracurricularCoachStatusHandler _extracurricularCoachStatusHandler;

        public ExtracurricularCoachStatusEndpoint
        (
        ExtracurricularCoachStatusHandler extracurricularCoachStatusHandler)
        {
            _extracurricularCoachStatusHandler = extracurricularCoachStatusHandler;
        }

        [FunctionName(nameof(ExtracurricularCoachStatusEndpoint.ExtracurricularCoachStatus))]
        [OpenApiOperation(tags: _tag, Summary = "Get Elective CoachStatus")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]        
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<CodeWithIdVm>))]
        public Task<IActionResult> ExtracurricularCoachStatus([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _extracurricularCoachStatusHandler.Execute(req, cancellationToken);
        }

    }
}
