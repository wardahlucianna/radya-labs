using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.GcCorner;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnGuidanceCounseling.GcCorner
{
    public class GcCornerEndPoint
    {
        private const string _route = "guidance-counseling/gc-corner";
        private const string _tag = "GC Corner";

        [FunctionName(nameof(GcCornerEndPoint.GetListGcCornerArticlePersonalWellBeing))]
        [OpenApiOperation(tags: _tag, Summary = "Get GC Corner Article Personal Well Being")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGcCornerWellBeingRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGcCornerWellBeingRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGcCornerWellBeingRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGcCornerWellBeingRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGcCornerWellBeingRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGcCornerWellBeingRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGcCornerWellBeingRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGcCornerWellBeingResult))]
        public Task<IActionResult> GetListGcCornerArticlePersonalWellBeing(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-article-personal-well-being")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGcCornerPersonalWellBeingHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(GcCornerEndPoint.GetListGcCornerGcLink))]
        [OpenApiOperation(tags: _tag, Summary = "Get GC Corner GC Link")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGcCornerGcLinkRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGcCornerGcLinkRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGcCornerGcLinkRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGcCornerGcLinkRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGcCornerGcLinkRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGcCornerGcLinkRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGcCornerGcLinkRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGcCornerGcLinkResult))]
        public Task<IActionResult> GetListGcCornerGcLink(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-gc-link")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGcCornerGcLinkHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(GcCornerEndPoint.GetListGcCornerCountryFact))]
        [OpenApiOperation(tags: _tag, Summary = "Get GC Corner Country Fact")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGcCornerCountryFactRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGcCornerCountryFactRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGcCornerCountryFactRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGcCornerCountryFactRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGcCornerCountryFactRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGcCornerCountryFactRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGcCornerCountryFactRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGcCornerCountryFactResult))]
        public Task<IActionResult> GetListGcCornerCountryFact(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-country-fact")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGcCornerCountryFactHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(GcCornerEndPoint.GetListGcCornerUsefulLink))]
        [OpenApiOperation(tags: _tag, Summary = "Get GC Corner Useful Link")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGcCornerUsefulLinkRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true )]
        [OpenApiParameter(nameof(GetGcCornerUsefulLinkRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGcCornerUsefulLinkRequest.IdRoleGroup), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGcCornerUsefulLinkRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGcCornerUsefulLinkRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGcCornerUsefulLinkRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGcCornerUsefulLinkRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGcCornerUsefulLinkRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGcCornerUsefulLinkRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGcCornerUsefulLinkRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGcCornerUsefulLinkRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGcCornerUsefulLinkResult))]
        public Task<IActionResult> GetListGcCornerUsefulLink(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-useful-link")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGcCornerUsefulLinkHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(GcCornerEndPoint.GetListGcCornerUniversityPortal))]
        [OpenApiOperation(tags: _tag, Summary = "Get GC Corner University Portal")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGcCornerUniversityPortalRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGcCornerUniversityPortalRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGcCornerUniversityPortalRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGcCornerUniversityPortalRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGcCornerUniversityPortalRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGcCornerUniversityPortalRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGcCornerUniversityPortalRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGcCornerUniversityPortalRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGcCornerUniversityPortalResult))]
        public Task<IActionResult> GetListGcCornerUniversityPortal(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-university-portal")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGcCornerUniversityPortalHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(GcCornerEndPoint.GetGcCornerYourCounselor))]
        [OpenApiOperation(tags: _tag, Summary = "Get GC Corner Your Counselor by Id Counselor")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGcCornerYourCounselorRequest.IdStudent), In = ParameterLocation.Query,Required =true)]
        [OpenApiParameter(nameof(GetGcCornerYourCounselorRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGcCornerYourCounselorResult))]
        public Task<IActionResult> GetGcCornerYourCounselor(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-your-counselor")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGcCornerYourCounselorHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
