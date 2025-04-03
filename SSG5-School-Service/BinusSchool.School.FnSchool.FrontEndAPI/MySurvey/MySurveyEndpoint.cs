using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.MySurvey;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.MySurvey
{
    public class MySurveyEndpoint
    {
        private const string _route = "my-survey";
        private const string _tag = "My Survey";

        private readonly MySurveyHandler _mySurveyHandler;
        public MySurveyEndpoint(MySurveyHandler mySurveyHandler)
        {
            _mySurveyHandler = mySurveyHandler;
        }

        [FunctionName(nameof(MySurveyEndpoint.GetMySurvey))]
        [OpenApiOperation(tags: _tag, Summary = "Get My Survey")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.GetAll), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMySurveyRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMySurveyRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMySurveyRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMySurveyRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMySurveyRequest.StartDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMySurveyRequest.EndDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMySurveyRequest.IdUserParent), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMySurveyResult[]))]
        public Task<IActionResult> GetMySurvey(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _mySurveyHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MySurveyEndpoint.GetMySurveyDetail))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMySurveyDetailRequest.IdPublishSurvey), In = ParameterLocation.Query, Required =true)]
        [OpenApiParameter(nameof(GetMySurveyDetailRequest.IdAcademicYear), In = ParameterLocation.Query, Required =true)]
        [OpenApiParameter(nameof(GetMySurveyDetailRequest.Semester), In = ParameterLocation.Query, Required =true)]
        [OpenApiParameter(nameof(GetMySurveyDetailRequest.IdSchool), In = ParameterLocation.Query, Required =true)]
        [OpenApiParameter(nameof(GetMySurveyDetailRequest.IdSurvey), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMySurveyDetailRequest.IdUser), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMySurveyDetailResult))]
        public Task<IActionResult> GetMySurveyDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DetailMySurveyHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MySurveyEndpoint.AddMySurvey))]
        [OpenApiOperation(tags: _tag, Summary = "Add MySurvey")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMySurveyRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AddMySurveyResult))]
        public Task<IActionResult> AddMySurvey(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MySurveyHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MySurveyEndpoint.UpdateMySurvey))]
        [OpenApiOperation(tags: _tag, Summary = "Update MySurvey")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateMySurveyRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateMySurvey(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MySurveyHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MySurveyEndpoint.DeleteMySurvey))]
        [OpenApiOperation(tags: _tag, Summary = "Delete MySurvey")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteMySurvey(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MySurveyHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MySurveyEndpoint.GetMySurveyLinkSurvey))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMySurveyLinkSurveyRequest.IdPublishSurvey), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMySurveyLinkSurveyRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMySurveyLinkSurveyRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMySurveyLinkSurveyRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMySurveyResult))]
        public Task<IActionResult> GetMySurveyLinkSurvey(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/link-publish-survey")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetMySurveyLinkSurveyHandler>();
            return handler.Execute(req, cancellationToken);
        }

    }
}
