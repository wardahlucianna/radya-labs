using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.User.FnBlocking.BlockingType;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.User.FnBlocking.BlockingType
{
    public class BlockingTypeEndPoint
    {
        private const string _route = "user-blocking/blocking-type";
        private const string _tag = "Blocking Type";

        [FunctionName(nameof(BlockingTypeEndPoint.GetBlockingType))]
        [OpenApiOperation(tags: _tag, Summary = "Get Blocking Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetBlockingTypeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetBlockingTypeRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBlockingTypeRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBlockingTypeRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetBlockingTypeRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetBlockingTypeRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBlockingTypeRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBlockingTypeRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetBlockingTypeResult))]
        public Task<IActionResult> GetBlockingType(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<BlockingTypeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BlockingTypeEndPoint.AddBlockingType))]
        [OpenApiOperation(tags: _tag, Summary = "Add Blocking Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddBlockingTypeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddBlockingType(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<BlockingTypeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BlockingTypeEndPoint.UpdateBlockingType))]
        [OpenApiOperation(tags: _tag, Summary = "Update Blocking Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateBlockingTypeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateBlockingType(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<BlockingTypeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BlockingTypeEndPoint.GetBlockingTypeDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Blocking Type Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetBlockingTypeDetailResult))]
        public Task<IActionResult> GetBlockingTypeDetail(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/{id}")] HttpRequest req,
        string id,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<BlockingTypeHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }

        [FunctionName(nameof(BlockingTypeEndPoint.DeleteBlockingType))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Blocking Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteBlockingType(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<BlockingTypeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BlockingTypeEndPoint.GetBlockingTypeMenu))]
        [OpenApiOperation(tags: _tag, Summary = "Get Blocking Type Menu")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetBlockingTypeMenuRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBlockingTypeMenuRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBlockingTypeMenuRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetBlockingTypeMenuRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetBlockingTypeMenuRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBlockingTypeMenuRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBlockingTypeMenuRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetBlockingTypeMenuResult))]
        public Task<IActionResult> GetBlockingTypeMenu(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/menu")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetBlockingTypeMenuHandler>();
            return handler.Execute(req, cancellationToken);
        }

    }
}
