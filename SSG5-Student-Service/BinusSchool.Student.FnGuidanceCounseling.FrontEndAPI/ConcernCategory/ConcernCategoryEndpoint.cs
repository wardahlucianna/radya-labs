using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ConcernCategory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnGuidanceCounseling.ConcernCategory
{
    public class ConcernCategoryEndpoint
    {
        private const string _route = "guidance-counseling/concern-category";
        private const string _tag = "Concern Category";

        private readonly ConcernCategoryHandler _concernCategoryHandler;
        public ConcernCategoryEndpoint(ConcernCategoryHandler concernCategoryHandler)
        {
            _concernCategoryHandler = concernCategoryHandler;
        }

        #region Concern Category
        [FunctionName(nameof(ConcernCategoryEndpoint.GetConcernCategory))]
        [OpenApiOperation(tags: _tag, Summary = "Get Concern Categories")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetConcernCategoryRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetConcernCategoryRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetConcernCategoryRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetConcernCategoryRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetConcernCategoryRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetConcernCategoryRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetConcernCategoryRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetConcernCategoryRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetConcernCategoryResult[]))]
        public Task<IActionResult> GetConcernCategory(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken
            )
        {
            return _concernCategoryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ConcernCategoryEndpoint.GetDetailConcernCategory))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Concern Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailConcernCategoryResult))]
        public Task<IActionResult> GetDetailConcernCategory(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            return _concernCategoryHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(ConcernCategoryEndpoint.AddConcernCategory))]
        [OpenApiOperation(tags: _tag, Summary = "Add Concern Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddConcernCategoryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddConcernCategory(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _concernCategoryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ConcernCategoryEndpoint.UpdateConcernCategory))]
        [OpenApiOperation(tags: _tag, Summary = "Update Concern Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateConcernCategoryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateConcernCategory(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _concernCategoryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ConcernCategoryEndpoint.DeleteConcernCategory))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Concern Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteConcernCategory(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _concernCategoryHandler.Execute(req, cancellationToken);
        }
        #endregion
    }
}
