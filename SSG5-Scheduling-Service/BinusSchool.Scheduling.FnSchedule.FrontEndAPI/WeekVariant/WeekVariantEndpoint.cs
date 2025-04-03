using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.WeekVariant;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.WeekVariant
{
    public class WeekVariantEndpoint
    {
        private const string _route = "schedule/week-variant";
        private const string _tag = "Week Variant";

        private readonly WeekVariantHandler _handler;

        public WeekVariantEndpoint(WeekVariantHandler handler)
        {
            _handler = handler;
        }

        [FunctionName(nameof(WeekVariantEndpoint.GetWeeklyVariants))]
        [OpenApiOperation(tags: _tag, Summary = "Get Week Variant List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetWeekVariantResult[]))]
        public Task<IActionResult> GetWeeklyVariants(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }
    }
}