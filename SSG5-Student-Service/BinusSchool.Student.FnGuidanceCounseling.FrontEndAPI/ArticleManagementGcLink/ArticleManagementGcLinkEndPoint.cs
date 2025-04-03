using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ArticleManagementGcLink;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnGuidanceCounseling.ArticleManagementGcLink
{
    public class ArticleManagementGcLinkEndPoint
    {
        private const string _route = "guidance-counseling/article-management-gc-link";
        private const string _tag = "Article Management GC Link";

        [FunctionName(nameof(ArticleManagementGcLinkEndPoint.GetListArticleManagementGcLink))]
        [OpenApiOperation(tags: _tag, Summary = "Get Article Management Gc Link")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetArticleManagementGcLinkRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetArticleManagementGcLinkRequest.GradeId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetArticleManagementGcLinkRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetArticleManagementGcLinkRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetArticleManagementGcLinkRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetArticleManagementGcLinkRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetArticleManagementGcLinkRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetArticleManagementGcLinkRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetArticleManagementGcLinkRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetArticleManagementGcLinkResult))]
        public Task<IActionResult> GetListArticleManagementGcLink(
                [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
                CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ArticleManagementGcLinkHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(ArticleManagementGcLinkEndPoint.AddArticleManagementGcLink))]
        [OpenApiOperation(tags: _tag, Summary = "Add Article Management Gc Link")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddArticleManagementGcLinkRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddArticleManagementGcLink(
             [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
             CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ArticleManagementGcLinkHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ArticleManagementGcLinkEndPoint.UpdateArticleManagementGcLink))]
        [OpenApiOperation(tags: _tag, Summary = "Update Article Management Gc Link")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateArticleManagementGcLinkRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateArticleManagementGcLink(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ArticleManagementGcLinkHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ArticleManagementGcLinkEndPoint.GetArticleManagementGcLinkDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Article Management Gc Link Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetArticleManagementGcLinkResult))]
        public Task<IActionResult> GetArticleManagementGcLinkDetail(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/{id}")] HttpRequest req,
        string id,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ArticleManagementGcLinkHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }

        [FunctionName(nameof(ArticleManagementGcLinkEndPoint.DeleteArticleManagementGcLink))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Article Management Gc Link")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteArticleManagementGcLink(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ArticleManagementGcLinkHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
