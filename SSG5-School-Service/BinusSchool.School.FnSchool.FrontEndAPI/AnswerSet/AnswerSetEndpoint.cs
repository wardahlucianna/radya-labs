using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.School.FnSchool.AnswerSet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.AnswerSet
{
    public class AnswerSetEndpoint
    {
        private const string _route = "answer-set";
        private const string _tag = "Answer Set";

        [FunctionName(nameof(AnswerSetEndpoint.GetAnswerSets))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAnswerSetRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAnswerSetRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAnswerSetRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAnswerSetRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAnswerSetRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAnswerSetRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAnswerSetRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAnswerSetRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AnswerSetResult))]
        public Task<IActionResult> GetAnswerSets(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AnswerSetHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(AnswerSetEndpoint.GetAnswerSetDetail))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AnswerSetDetailResult))]
        public Task<IActionResult> GetAnswerSetDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AnswerSetHandler>();
            return handler.Execute(req, cancellationToken, false, "id".WithValue(id));
        }

        [FunctionName(nameof(AnswerSetEndpoint.AddAnswerSet))]
        [OpenApiOperation(tags: _tag, Summary = "Add AnswerSet")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddAnswerSetRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddAnswerSet(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AnswerSetHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AnswerSetEndpoint.UpdateAnswerSet))]
        [OpenApiOperation(tags: _tag, Summary = "Update AnswerSet")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateAnswerSetRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateAnswerSet(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AnswerSetHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AnswerSetEndpoint.DeleteAnswerSet))]
        [OpenApiOperation(tags: _tag, Summary = "Delete AnswerSet")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteAnswerSet(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AnswerSetHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AnswerSetEndpoint.GetCopyListAnswerSets))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCopyListAnswerSetRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetCopyListAnswerSetRequest.CopyToIdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetCopyListAnswerSetRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCopyListAnswerSetRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCopyListAnswerSetRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetCopyListAnswerSetRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetCopyListAnswerSetRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCopyListAnswerSetRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCopyListAnswerSetRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AnswerSetResult))]
        public Task<IActionResult> GetCopyListAnswerSets(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/list-answer-set")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetCopyListAnswerSetHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(AnswerSetEndpoint.CopyListAnswerSet))]
        [OpenApiOperation(tags: _tag, Summary = "Copy AnswerSet To Next AY")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CopyListAnswerSetRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> CopyListAnswerSet(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/list-answer-set")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CopyListAnswerSetHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
