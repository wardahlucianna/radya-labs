using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Teaching.FnAssignment.NonTeachingLoad;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Teaching.FnAssignment.NonTeachingLoad
{
    public class NonTeachingLoadEndpoint
    {
        private const string _route = "assignment/non-teaching-load";
        private const string _tag = "Non Teaching Load";

        [FunctionName(nameof(NonTeachingLoadEndpoint.GetNonTeachingLoads))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetNonTeachLoadRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetNonTeachLoadRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetNonTeachLoadRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetNonTeachLoadRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetNonTeachLoadRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetNonTeachLoadRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetNonTeachLoadRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetNonTeachLoadRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetNonTeachLoadRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetNonTeachLoadRequest.Category), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetNonTeachLoadResult))]
        public Task<IActionResult> GetNonTeachingLoads(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetNonTeachingLoadHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(NonTeachingLoadEndpoint.GetCopyNonTeachingLoads))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCopyNonTeachingLoadRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCopyNonTeachingLoadRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCopyNonTeachingLoadRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetCopyNonTeachingLoadRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetCopyNonTeachingLoadRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCopyNonTeachingLoadRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCopyNonTeachingLoadRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetCopyNonTeachingLoadRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetCopyNonTeachingLoadRequest.IdAcadYearTarget), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCopyNonTeachingLoadRequest.IdAcadYearSource), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCopyNonTeachingLoadRequest.Category), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCopyNonTeachingLoadResult))]
        public Task<IActionResult> GetCopyNonTeachingLoads(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-copy")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetCopyNonTeachingLoadHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(NonTeachingLoadEndpoint.GetNonTeachingLoadDetail))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetNonTeachLoadDetailResult))]
        public Task<IActionResult> GetNonTeachingLoadDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<NonTeachingLoadHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }

        [FunctionName(nameof(NonTeachingLoadEndpoint.AddNonTeachingLoad))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddNonTeachLoadRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddNonTeachingLoad(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddNonTeachingLoadHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(NonTeachingLoadEndpoint.UpdateNonTeachingLoad))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateNonTeachLoadRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateNonTeachingLoad(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateNonTeachingLoadHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(NonTeachingLoadEndpoint.DeleteNonTeachingLoad))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteNonTeachingLoad(
           [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<NonTeachingLoadHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(NonTeachingLoadEndpoint.GetPreviousNonTeachingLoad))]
        [OpenApiOperation(tags: _tag, Summary = "Get hierarchy for non teaching load from previous AY")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetPreviousHierarchyNonTeachingLoadRequest.IdAcadyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPreviousHierarchyNonTeachingLoadRequest.IdPosition), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPreviousHierarchyNonTeachingLoadRequest.Category), In = ParameterLocation.Query, Type = typeof(AcademicType))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetPreviousHierarchyNonTeachingLoadResult))]
        public Task<IActionResult> GetPreviousNonTeachingLoad(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-previous")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetPreviousHierarchyNonTeachingLoadHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(NonTeachingLoadEndpoint.CopyNonTeachingLoad))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CopyNonTeachingLoadRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> CopyNonTeachingLoad(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/copy")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CopyNonTeachingLoadHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
