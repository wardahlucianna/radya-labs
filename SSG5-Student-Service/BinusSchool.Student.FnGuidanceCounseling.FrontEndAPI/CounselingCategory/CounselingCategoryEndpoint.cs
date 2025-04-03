using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselingCategory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnGuidanceCounseling.CounselingCategory
{
    public class CounselingCategoryEndpoint
    {
        private const string _route = "guidance-counseling/counseling-category";
        private const string _tag = "Counseling Category";

        private readonly CounselingCategoryHandler _counselingCategoryHandler;
        public CounselingCategoryEndpoint(CounselingCategoryHandler counselingCategoryHandler)
        {
            _counselingCategoryHandler = counselingCategoryHandler;
        }

        #region Counseling Category
        [FunctionName(nameof(CounselingCategoryEndpoint.GetCounselingCategory))]
        [OpenApiOperation(tags: _tag, Summary = "Get Counseling Categories")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCounselingCategoryRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetCounselingCategoryRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselingCategoryRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselingCategoryRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetCounselingCategoryRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetCounselingCategoryRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselingCategoryRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselingCategoryRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCounselingCategoryResult[]))]
        public Task<IActionResult> GetCounselingCategory(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken
            )
        {
            return _counselingCategoryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CounselingCategoryEndpoint.GetDetailCounselingCategory))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Counseling Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailCounselingCategoryResult))]
        public Task<IActionResult> GetDetailCounselingCategory(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            return _counselingCategoryHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(CounselingCategoryEndpoint.AddCounselingCategory))]
        [OpenApiOperation(tags: _tag, Summary = "Add Counseling Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddCounselingCategoryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddCounselingCategory(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _counselingCategoryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CounselingCategoryEndpoint.UpdateCounselingCategory))]
        [OpenApiOperation(tags: _tag, Summary = "Update Counseling Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateCounselingCategoryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateCounselingCategory(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _counselingCategoryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CounselingCategoryEndpoint.DeleteCounselingCategory))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Counseling Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteCounselingCategory(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _counselingCategoryHandler.Execute(req, cancellationToken);
        }
        #endregion
    }
}
