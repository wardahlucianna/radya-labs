using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.Pathway;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.Pathway
{
    public class PathwayEndpoint
    {
        private const string _route = "school/pathway";
        private const string _tag = "School Pathway";

        private readonly PathwayHandler _handler;
        private readonly CopyPathwayHandler _copyHandler;
        private readonly PathwayGradeHandler _pathwayGradeHandler;
        private readonly SummaryPathwayGradeHandler _summarypathwayGradeHandler;

        public PathwayEndpoint(PathwayHandler handler,
            CopyPathwayHandler copyHandler,
            PathwayGradeHandler pathwayGradeHandler, SummaryPathwayGradeHandler summarypathwayGradeHandler)
        {
            _handler = handler;
            _copyHandler = copyHandler;
            _pathwayGradeHandler = pathwayGradeHandler;
            _summarypathwayGradeHandler = summarypathwayGradeHandler;
        }

        [FunctionName(nameof(PathwayEndpoint.GetPathways))]
        [OpenApiOperation(tags: _tag, Summary = "Get Pathway List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetPathwayRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetPathwayRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPathwayRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPathwayRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetPathwayRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetPathwayRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPathwayRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPathwayRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetPathwayRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPathwayRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPathwayRequest.IdPathway), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetPathwayResult))]
        public Task<IActionResult> GetPathways(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PathwayEndpoint.GetPathwayDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Pathway Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetPathwayDetailResult))]
        public Task<IActionResult> GetPathwayDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(PathwayEndpoint.AddPathway))]
        [OpenApiOperation(tags: _tag, Summary = "Add Pathway")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddPathwayRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddPathway(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PathwayEndpoint.UpdatePathway))]
        [OpenApiOperation(tags: _tag, Summary = "Update Pathway")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdatePathwayRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdatePathway(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PathwayEndpoint.DeletePathway))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Pathway")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeletePathway(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PathwayEndpoint.CopyPathway))]
        [OpenApiOperation(tags: _tag, Summary = "Copy Pathway")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CopyPathwayRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> CopyPathway(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = "school/copy-pathway")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _copyHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PathwayEndpoint.GetPathwayByGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Get pathway by grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetPathwayGradeRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPathwayGradeRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPathwayGradeRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetPathwayGradeRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetPathwayGradeRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPathwayGradeRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPathwayGradeRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetPathwayGradeRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CodeWithIdVm))]
        public Task<IActionResult> GetPathwayByGrade(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "school/pathway-grade")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _pathwayGradeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PathwayEndpoint.GetSummaryPathwayByGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary pathway by grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetPathwayGradeRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPathwayGradeRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPathwayGradeRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetPathwayGradeRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetPathwayGradeRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPathwayGradeRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPathwayGradeRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetPathwayGradeRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CodeWithIdVm))]
        public Task<IActionResult> GetSummaryPathwayByGrade(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "school/summary-pathway-grade")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _summarypathwayGradeHandler.Execute(req, cancellationToken);
        }
    }
}
