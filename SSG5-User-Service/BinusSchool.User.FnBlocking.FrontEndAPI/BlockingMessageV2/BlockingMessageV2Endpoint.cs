using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.User.FnBlocking.BlockingMessageV2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.User.FnBlocking.BlockingMessageV2
{
    public class BlockingMessageV2Endpoint
    {
        private const string _route = "user-blocking/blocking-message-v2";
        private const string _tag = "Blocking Message V2";

        [FunctionName(nameof(BlockingMessageV2Endpoint.GetBlockingMessageV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Blocking Message")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetBlockingMessageRequestV2.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetBlockingMessageRequestV2.IdCategory), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBlockingMessageRequestV2.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBlockingMessageRequestV2.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBlockingMessageRequestV2.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetBlockingMessageRequestV2.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetBlockingMessageRequestV2.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBlockingMessageRequestV2.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBlockingMessageRequestV2.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetBlockingMessageResultV2))]
        public Task<IActionResult> GetBlockingMessageV2(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<BlockingMessageHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BlockingMessageV2Endpoint.GetBlockingMessageDetailV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Blocking Category Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetBlockingMessageResultV2))]
        public Task<IActionResult> GetBlockingMessageDetailV2(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/{id}")] HttpRequest req,
        string id,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<BlockingMessageHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }

        [FunctionName(nameof(BlockingMessageV2Endpoint.AddBlockingMessageV2))]
        [OpenApiOperation(tags: _tag, Summary = "Add Blocking Message")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddBlockingMessageRequestV2))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddBlockingMessageV2(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<BlockingMessageHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BlockingMessageV2Endpoint.UpdateBlockingMessageV2))]
        [OpenApiOperation(tags: _tag, Summary = "Update Blocking Message")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateBlockingMessageRequestV2))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateBlockingMessageV2(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<BlockingMessageHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BlockingMessageV2Endpoint.DeleteBlockingMessageV2))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Blocking Message")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteBlockingMessageV2(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<BlockingMessageHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
