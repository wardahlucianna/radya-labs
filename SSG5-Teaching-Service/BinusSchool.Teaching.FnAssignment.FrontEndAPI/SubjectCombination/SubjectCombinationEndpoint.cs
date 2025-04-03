using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.SubjectCombination;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Teaching.FnAssignment.SubjectCombination
{
    public class SubjectCombinationEndpoint
    {
        private const string _route = "subject-combination";
        private const string _tag = "Subject Combination";

        [FunctionName(nameof(GetSubjectCombination))]
        [OpenApiOperation(tags: _tag, Summary = "Get Subject Combination")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSubjectCombinationRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectCombinationRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectCombinationRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSubjectCombinationRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSubjectCombinationRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectCombinationRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectCombinationRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetSubjectCombinationRequest.IdAcadyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSubjectCombinationRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSubjectCombinationResult[]))]
        public Task<IActionResult> GetSubjectCombination(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req, 
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSubjectCombinationHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AddSubjectCombination))]
        [OpenApiOperation(tags: _tag, Summary = "Add Subject Combination")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddSubjectCombination))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddSubjectCombination(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddSubjectCombinationHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetSubjectCombinationMetadata))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IdCollection))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSubjectCombinationTimetableResult))]
        public Task<IActionResult> GetSubjectCombinationMetadata(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/metadata")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSubjectCombinationMetadataHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
