using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.Level;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.Level
{
    public class LevelEndpoint
    {
        private const string _route = "school/level";
        private const string _tag = "School Level";

        private readonly LevelHandler _handler;
        private readonly LevelAcadyearHandler _acadyearHandler;
        private readonly LevelCodeHandler _levelCodeHandler;
        public LevelEndpoint(LevelHandler handler,
            LevelAcadyearHandler acadyearHandler,
            LevelCodeHandler levelCodeHandler)
        {
            _handler = handler;
            _acadyearHandler = acadyearHandler;
            _levelCodeHandler = levelCodeHandler;
        }

        [FunctionName(nameof(LevelEndpoint.GetLevels))]
        [OpenApiOperation(tags: _tag, Summary = "Get Level List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLevelRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLevelRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLevelRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetLevelRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetLevelRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLevelRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLevelRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetLevelRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLevelRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLevelRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLevelResult))]
        public Task<IActionResult> GetLevels(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LevelEndpoint.GetLevelDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Level Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLevelDetailResult))]
        public Task<IActionResult> GetLevelDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(LevelEndpoint.AddLevel))]
        [OpenApiOperation(tags: _tag, Summary = "Add Level")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddLevelRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddLevel(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(LevelEndpoint.UpdateLevel))]
        [OpenApiOperation(tags: _tag, Summary = "Update Level")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateLevelRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateLevel(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LevelEndpoint.DeleteLevel))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Level")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteLevel(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LevelEndpoint.GetLevelsByAcadYear))]
        [OpenApiOperation(tags: _tag, Summary = "Get Levels by Academic Year")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLevelRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLevelRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLevelRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetLevelRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetLevelRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLevelRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLevelRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetLevelRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLevelRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLevelRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLevelResult))]
        public Task<IActionResult> GetLevelsByAcadYear(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "school/level-acadyear")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _acadyearHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LevelEndpoint.GetLevelCodeList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Level Code List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLevelCodeRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLevelCodeRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLevelCodeRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetLevelCodeRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetLevelCodeRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLevelCodeRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLevelCodeRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetLevelCodeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLevelCodeRequest.CodeLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLevelCodeRequest.CodeAcademicYear), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLevelCodeResult))]
        public Task<IActionResult> GetLevelCodeList(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-code")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _levelCodeHandler.Execute(req, cancellationToken);
        }
    }
}
