using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Award;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.Award
{
    public class AwardEndpoint
    {
        private const string _route = "schedule/award";
        private const string _tag = "Award";

        private readonly AwardHandler _handler;
        public AwardEndpoint(AwardHandler handler)
        {
            _handler = handler;
        }

        [FunctionName(nameof(AwardEndpoint.GetAwards))]
        [OpenApiOperation(tags: _tag, Summary = "Get Award List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAwardRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAwardRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAwardRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAwardRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAwardRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAwardRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAwardRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetAwardRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AwardResult))]
        public Task<IActionResult> GetAwards(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AwardEndpoint.GetAwardDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Award Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AwardResult))]
        public Task<IActionResult> GetAwardDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(AwardEndpoint.AddAward))]
        [OpenApiOperation(tags: _tag, Summary = "Add Award")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CreateAwardRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddAward(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(AwardEndpoint.UpdateAward))]
        [OpenApiOperation(tags: _tag, Summary = "Update Award")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateAwardRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateAward(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AwardEndpoint.DeleteAward))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Award")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteAward(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AwardEndpoint.SetAwardRecommendationStatus))]
        [OpenApiOperation(tags: _tag, Summary = "Set Recommendation Award")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SetAwardRecommendationStatusRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SetAwardRecommendationStatus(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/set-recommendation")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handlerSetAward = req.HttpContext.RequestServices.GetService<SetAwardRecommendationStatusHandler>();
            return handlerSetAward.Execute(req, cancellationToken);
        }
    }
}
