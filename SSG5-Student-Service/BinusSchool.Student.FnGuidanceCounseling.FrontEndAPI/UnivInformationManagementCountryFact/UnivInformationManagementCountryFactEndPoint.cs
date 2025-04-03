using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementCountryFact;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnGuidanceCounseling.UnivInformationManagementCountryFact
{
    public class UnivInformationManagementCountryFactEndPoint
    {
        private const string _route = "guidance-counseling/university-information-management-country-fact";
        private const string _tag = "University Information Management Country Fact";

        [FunctionName(nameof(UnivInformationManagementCountryFactEndPoint.GetListUnivInformationManagementCountryFact))]
        [OpenApiOperation(tags: _tag, Summary = "Get University Information Management Country Fact")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUnivInformationManagementCountryFactRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnivInformationManagementCountryFactRequest.LevelId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnivInformationManagementCountryFactRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnivInformationManagementCountryFactRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnivInformationManagementCountryFactRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUnivInformationManagementCountryFactRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUnivInformationManagementCountryFactRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnivInformationManagementCountryFactRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnivInformationManagementCountryFactRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUnivInformationManagementCountryFactResult))]
        public Task<IActionResult> GetListUnivInformationManagementCountryFact(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UnivInformationManagementCountryFactHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(UnivInformationManagementCountryFactEndPoint.AddUnivInformationManagementCountryFact))]
        [OpenApiOperation(tags: _tag, Summary = "Add University Information Management Country Fact")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddUnivInformationManagementCountryFactRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddUnivInformationManagementCountryFact(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UnivInformationManagementCountryFactHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UnivInformationManagementCountryFactEndPoint.UpdateUnivInformationManagementCountryFact))]
        [OpenApiOperation(tags: _tag, Summary = "Update University Information Management Country Fact")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateUnivInformationManagementCountryFactRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateUnivInformationManagementCountryFact(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UnivInformationManagementCountryFactHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UnivInformationManagementCountryFactEndPoint.GetUnivInformationManagementCountryFactDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get University Information Management Country Fact Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUnivInformationManagementCountryFactResult))]
        public Task<IActionResult> GetUnivInformationManagementCountryFactDetail(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/{id}")] HttpRequest req,
        string id,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UnivInformationManagementCountryFactHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }

        [FunctionName(nameof(UnivInformationManagementCountryFactEndPoint.DeleteUnivInformationManagementCountryFact))]
        [OpenApiOperation(tags: _tag, Summary = "Delete University Information Management Country Fact")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteUnivInformationManagementCountryFact(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UnivInformationManagementCountryFactHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
