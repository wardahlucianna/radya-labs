using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.User.FnBlocking.BlockingMessage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.User.FnBlocking.BlockingMessage
{
    public class BlockingMessageEndpoint
    {
        private const string _route = "user-blocking/blocking-message";
        private const string _tag = "Blocking Message";

        [FunctionName(nameof(BlockingMessageEndpoint.GetBlockingMessage))]
        [OpenApiOperation(tags: _tag, Summary = "Get Blocking Message")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetBlockingMessageRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetBlockingMessageResult))]
        public Task<IActionResult> GetBlockingMessage(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetBlockingMessageHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BlockingMessageEndpoint.GetBlockingMessageDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Blocking Message Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetBlockingMessageResult))]
        public Task<IActionResult> GetBlockingMessageDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetBlockingMessageDetailHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }

        [FunctionName(nameof(BlockingMessageEndpoint.UpdateBlockingMessage))]
        [OpenApiOperation(tags: _tag, Summary = "Update Blocking Message")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateBlockingMessageRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateBlockingMessage(
             [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
             CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateBlockingMessageHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
