using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ArticleManagementPersonalWellBeing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnGuidanceCounseling.ArticleManagementPersonalWellBeing
{
    public class ArticleManagementPersonalWellBeingEndPoint
    {
        private const string _route = "guidance-counseling/article-management-personal-well-being";
        private const string _tag = "Article Management Personal Well Being";

        [FunctionName(nameof(ArticleManagementPersonalWellBeingEndPoint.GetListArticleManagementPersonalWellBeing))]
        [OpenApiOperation(tags: _tag, Summary = "Get Article Management Personal Well Being")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetArticleManagementPersonalWellBeingRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetArticleManagementPersonalWellBeingRequest.LevelId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetArticleManagementPersonalWellBeingRequest.ViewFor), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetArticleManagementPersonalWellBeingRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetArticleManagementPersonalWellBeingRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetArticleManagementPersonalWellBeingRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetArticleManagementPersonalWellBeingRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetArticleManagementPersonalWellBeingRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetArticleManagementPersonalWellBeingRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetArticleManagementPersonalWellBeingRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetArticleManagementPersonalWellBeingResult))]
        public Task<IActionResult> GetListArticleManagementPersonalWellBeing(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ArticleManagementPersonalWellBeingHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(ArticleManagementPersonalWellBeingEndPoint.AddArticleManagementPersonalWellBeing))]
        [OpenApiOperation(tags: _tag, Summary = "Add Article Management Personal Well Being")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddArticleManagementPersonalWellBeingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddArticleManagementPersonalWellBeing(
             [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
             CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ArticleManagementPersonalWellBeingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ArticleManagementPersonalWellBeingEndPoint.UpdateArticleManagementPersonalWellBeing))]
        [OpenApiOperation(tags: _tag, Summary = "Update Article Management Personal Well Being")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateArticleManagementPersonalWellBeingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateArticleManagementPersonalWellBeing(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ArticleManagementPersonalWellBeingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ArticleManagementPersonalWellBeingEndPoint.GetArticleManagementPersonalWellBeingDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Article Management Personal Well Being Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetArticleManagementPersonalWellBeingResult))]
        public Task<IActionResult> GetArticleManagementPersonalWellBeingDetail(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/{id}")] HttpRequest req,
        string id,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ArticleManagementPersonalWellBeingHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }

        [FunctionName(nameof(ArticleManagementPersonalWellBeingEndPoint.DeleteArticleManagementPersonalWellBeing))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Article Management Personal Well Being")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteArticleManagementPersonalWellBeing(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ArticleManagementPersonalWellBeingHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
