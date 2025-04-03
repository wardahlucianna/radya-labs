using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.CheckUsage
{
    public class CheckUsageEndpoint
    {
        private const string _route = "schedule/check-usage";
        private const string _tag = "Check Usage";

        [FunctionName(nameof(CheckUsageEndpoint.CheckUsagePathwayDetail))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(IdCollection.Ids), In = ParameterLocation.Query, Type = typeof(string[]), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IDictionary<string, bool>))]
        public Task<IActionResult> CheckUsagePathwayDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/pathway-detail")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CheckUsagePathwayDetailHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}