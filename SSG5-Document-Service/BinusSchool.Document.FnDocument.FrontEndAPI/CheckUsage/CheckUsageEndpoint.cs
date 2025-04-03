using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Document.FnDocument.CheckUsage
{
    public class CheckUsageEndpoint
    {
        private const string _route = "document/check-usage";
        private const string _tag = "Check Usage";

        [FunctionName(nameof(CheckUsageEndpoint.CheckUsageTerm))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(bool))]
        public Task<IActionResult> CheckUsageTerm(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/term/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CheckUsageTermHandler>();
            return handler.Execute(req, cancellationToken, false, keyValues: "id".WithValue(id));
        }

        [FunctionName(nameof(CheckUsageEndpoint.CheckUsageSubject))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(bool))]
        public Task<IActionResult> CheckUsageSubject(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/subject/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CheckUsageSubjectHandler>();
            return handler.Execute(req, cancellationToken, false, keyValues: "id".WithValue(id));
        }
    }
}