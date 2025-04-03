using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.User.FnBlocking.BlockingCategory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.User.FnBlocking.BlockingCategory
{
    public class BlockingCategoryEndPoint
    {
        private const string _route = "user-blocking/blocking-category";
        private const string _tag = "Blocking Category";

        [FunctionName(nameof(BlockingCategoryEndPoint.GetBlockingCategory))]
        [OpenApiOperation(tags: _tag, Summary = "Get Blocking Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetBlockingCategoryRequest.IdSchool), In = ParameterLocation.Query,Required = true)]
        [OpenApiParameter(nameof(GetBlockingCategoryRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBlockingCategoryRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBlockingCategoryRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetBlockingCategoryRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetBlockingCategoryRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBlockingCategoryRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBlockingCategoryRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetBlockingCategoryResult))]
        public Task<IActionResult> GetBlockingCategory(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<BlockingCategoryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BlockingCategoryEndPoint.AddBlockingCategory))]
        [OpenApiOperation(tags: _tag, Summary = "Add Blocking Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddBlockingCategoryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddBlockingCategory(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<BlockingCategoryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BlockingCategoryEndPoint.UpdateBlockingCategory))]
        [OpenApiOperation(tags: _tag, Summary = "Update Blocking Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateBlockingCategoryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateBlockingCategory(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<BlockingCategoryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BlockingCategoryEndPoint.GetBlockingCategoryDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Blocking Category Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetBlockingCategoryDetailResult))]
        public Task<IActionResult> GetBlockingCategoryDetail(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/{id}")] HttpRequest req,
        string id,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<BlockingCategoryHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }

        [FunctionName(nameof(BlockingCategoryEndPoint.DeleteBlockingCategory))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Blocking Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteBlockingCategory(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<BlockingCategoryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BlockingCategoryEndPoint.GetBlockingCategoryByUser))]
        [OpenApiOperation(tags: _tag, Summary = "Get Blocking Category By User")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetBlockingByUserRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetBlockingByUserRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBlockingByUserRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBlockingByUserRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetBlockingByUserRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetBlockingByUserRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBlockingByUserRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBlockingByUserRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetBlockingByUserResult))]
        public Task<IActionResult> GetBlockingCategoryByUser(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/by-user")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetBlockingCatagoryByUserHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
