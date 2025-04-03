using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementUsefulLink;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnGuidanceCounseling.UnivInformationManagementUsefulLink
{
    public class UnivInformationManagementUsefulLinkEndPoint
    {
        private const string _route = "guidance-counseling/university-information-management-useful-link";
        private const string _tag = "University Information Management Useful Link";

        [FunctionName(nameof(UnivInformationManagementUsefulLinkEndPoint.GetListUnivInformationManagementUsefulLink))]
        [OpenApiOperation(tags: _tag, Summary = "Get University Information Management Useful Link")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUnivInformationManagementUsefulLinkRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnivInformationManagementUsefulLinkRequest.GradeId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnivInformationManagementUsefulLinkRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnivInformationManagementUsefulLinkRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnivInformationManagementUsefulLinkRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUnivInformationManagementUsefulLinkRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUnivInformationManagementUsefulLinkRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnivInformationManagementUsefulLinkRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnivInformationManagementUsefulLinkRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUnivInformationManagementUsefulLinkResult))]
        public Task<IActionResult> GetListUnivInformationManagementUsefulLink(
                [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
                CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UnivInformationManagementUsefulLinkHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(UnivInformationManagementUsefulLinkEndPoint.AddUnivInformationManagementUsefulLink))]
        [OpenApiOperation(tags: _tag, Summary = "Add University Information Management Useful Link")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddUnivInformationManagementUsefulLinkRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddUnivInformationManagementUsefulLink(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UnivInformationManagementUsefulLinkHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UnivInformationManagementUsefulLinkEndPoint.UpdateUnivInformationManagementUsefulLink))]
        [OpenApiOperation(tags: _tag, Summary = "Update University Information Management Useful Link")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateUnivInformationManagementUsefulLinkRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateUnivInformationManagementUsefulLink(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UnivInformationManagementUsefulLinkHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UnivInformationManagementUsefulLinkEndPoint.GetUnivInformationManagementUsefulLinkDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get University Information Management Useful Link Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUnivInformationManagementUsefulLinkResult))]
        public Task<IActionResult> GetUnivInformationManagementUsefulLinkDetail(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/{id}")] HttpRequest req,
        string id,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UnivInformationManagementUsefulLinkHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }

        [FunctionName(nameof(UnivInformationManagementUsefulLinkEndPoint.DeleteUnivInformationManagementUsefulLink))]
        [OpenApiOperation(tags: _tag, Summary = "Delete University Information Management Useful Link")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteUnivInformationManagementUsefulLink(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UnivInformationManagementUsefulLinkHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
