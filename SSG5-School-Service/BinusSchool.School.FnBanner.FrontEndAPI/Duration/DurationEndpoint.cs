using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.School.FnBanner.Duration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnBanner.Duration
{
    public class DurationEndpoint
    {
        private const string _tag = "Duration";

        private readonly GetBannerDurationHandler _handler;

        public DurationEndpoint(GetBannerDurationHandler handler)
        {
            _handler = handler;
        }

        [FunctionName(nameof(DurationEndpoint.GetDurationBanner))]
        [OpenApiOperation(tags: _tag, Summary = "Get Banner Duration")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetBannerDurationRequest.Type), In = ParameterLocation.Query, Required = true, Type = typeof(TypeDuration))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetBannerDurationResult))]
        public Task<IActionResult> GetDurationBanner(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "banner/duration")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetBannerDurationHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
