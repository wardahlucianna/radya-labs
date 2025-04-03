using System;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Persistence.TeachingDb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Teaching.FnSync.RetrySync
{
    public class RetrySyncEndpoint : FunctionsSyncRefTableHandler2<TeachingDbContext>
    {
        public RetrySyncEndpoint(IServiceProvider provider) : base(provider)
        {
        }

        //[FunctionName(nameof(Retry))]
        [OpenApiOperation(tags: Tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key",
            In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(RetrySyncRequest.HubName), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(RetrySyncRequest.RowKey), In = ParameterLocation.Query, Required = true)]
        public Task<IActionResult> Retry(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = Route)]
            HttpRequest req,
            CancellationToken cancellationToken)
        {
            return RetrySynchronize(req, cancellationToken);
        }
    }
}
