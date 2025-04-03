using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Workflow.FnSync.HealthCheck
{
    public class HealthCheckEndpoint : FunctionsHealthCheckHandler
    {
        [FunctionName(nameof(GetHealthCheck))]
        [OpenApiOperation(tags: Tag)]
        [OpenApiResponseWithoutBody(System.Net.HttpStatusCode.OK)]
        public Task<IActionResult> GetHealthCheck(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route)] HttpRequest req,
            ILogger log,
            CancellationToken cancellationToken)
        {
            return ExecuteHealthCheck(req, log, cancellationToken);
        }
    }
}