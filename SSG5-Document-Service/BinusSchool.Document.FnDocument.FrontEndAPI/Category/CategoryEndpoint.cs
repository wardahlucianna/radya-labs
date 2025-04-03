using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Document.FnDocument.Category;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Document.FnDocument.Category
{
    public class CategoryEndpoint
    {
        private const string _route = "document/category";
        private const string _tag = "Document Category";

        private readonly CategoryHandler _handler;

        public CategoryEndpoint(CategoryHandler handler)
        {
            _handler = handler;
        }

        [FunctionName(nameof(CategoryEndpoint.GetDocumentCategories))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        //[OpenApiParameter(nameof(GetCategoryRequest.DocumentType), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCategoryResult))]
        public Task<IActionResult> GetDocumentCategories(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }
        
        [FunctionName(nameof(CategoryEndpoint.GetDocumentCategoryDetail))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCategoryDetailResult))]
        public Task<IActionResult> GetDocumentCategoryDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }
        
        // [FunctionName(nameof(CategoryEndpoint.AddDocumentCategory))]
        // [OpenApiOperation(tags: _tag)]
        // [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        // [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        // [OpenApiRequestBody("application/json", typeof(AddCategoryRequest))]
        // [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        // public Task<IActionResult> AddDocumentCategory(
        //     [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
        //     CancellationToken cancellationToken)
        // {
        //     return _handler.Execute(req, cancellationToken);
        // }
        
        // [FunctionName(nameof(CategoryEndpoint.UpdateDocumentCategory))]
        // [OpenApiOperation(tags: _tag)]
        // [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        // [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        // [OpenApiRequestBody("application/json", typeof(UpdateCategoryRequest))]
        // [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        // public Task<IActionResult> UpdateDocumentCategory(
        //     [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
        //     CancellationToken cancellationToken)
        // {
        //     return _handler.Execute(req, cancellationToken);
        // }
        
        // [FunctionName(nameof(CategoryEndpoint.DeleteDocumentCategory))]
        // [OpenApiOperation(tags: _tag)]
        // [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        // [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        // [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        // [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        // public Task<IActionResult> DeleteDocumentCategory(
        //     [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        //     CancellationToken cancellationToken)
        // {
        //     return _handler.Execute(req, cancellationToken);
        // }
    }
}
