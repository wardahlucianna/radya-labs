
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Teaching.FnAssignment.Position;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Teaching.FnAssignment.Position
{
    public class LtPositionEndpoint
    {
        private const string _route = "assignment/position";
        private const string _tag = "Literatur Position";

        private readonly LtPositionHandler _handler;

        public LtPositionEndpoint(LtPositionHandler handler)
        {
            _handler = handler;
        }

        [FunctionName(nameof(LtPositionEndpoint.GetPositions))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(PositionGetRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(PositionGetRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(PositionGetRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(PositionGetRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(PositionGetRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(PositionGetRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(PositionGetRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(PositionGetResult))]
        public Task<IActionResult> GetPositions(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }
    }
}
