using System;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Common.Functions.Handler
{
    public abstract class FunctionsHealthCheckHandler
    {
        protected const string Route = "healthcheck";
        protected const string Tag = "Health Checks";

        protected async Task<IActionResult> ExecuteHealthCheck(HttpRequest request, ILogger logger, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("[HealthCheck] Received heartbeat request");

            var healthCheckService = request.HttpContext.RequestServices.GetService<HealthCheckService>();
            if (healthCheckService is null)
                throw new InvalidOperationException("Health Check is not registered. Please call AddFunctionsHealthChecks method in Startup class.");

            var status = await healthCheckService.CheckHealthAsync(cancellationToken);

            return new OkObjectResult(status);
        }
    }
}