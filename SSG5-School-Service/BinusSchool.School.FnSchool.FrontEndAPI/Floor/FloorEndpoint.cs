using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.Floor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.Floor
{
    public class FloorEndpoint
    {
        private const string _route = "school/floor";
        private const string _tag = "School Floor";

        private readonly GetDDLFloorHandler _getDDLFloorHandler;
        private readonly GetListFloorHandler _getListFloorHandler;
        private readonly SaveFloorHandler _saveFloorHandler;
        private readonly DeleteFloorHandler _deleteFloorHandler;
        private readonly GetDetailFloorHandler _getDetailFloorHandler;

        public FloorEndpoint(
            GetDDLFloorHandler getDDLFloorHandler,
            GetListFloorHandler getListFloorHandler,
            SaveFloorHandler saveFloorHandler,
            DeleteFloorHandler deleteFloorHandler,
            GetDetailFloorHandler getDetailFloorHandler)
        {
            _getDDLFloorHandler = getDDLFloorHandler;
            _getListFloorHandler = getListFloorHandler;
            _saveFloorHandler = saveFloorHandler;
            _deleteFloorHandler = deleteFloorHandler;
            _getDetailFloorHandler = getDetailFloorHandler;
        }

        [FunctionName(nameof(GetDDLFloor))]
        [OpenApiOperation(tags: _tag, Summary = "Get DDL Floor")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDDLFloorRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDDLFloorRequest.IdBuilding), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetDDLFloorResponse>))]
        public Task<IActionResult> GetDDLFloor(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-ddl-floor")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _getDDLFloorHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetListFloor))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Floor")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListFloorRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListFloorResult>))]
        public Task<IActionResult> GetListFloor([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-floor")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getListFloorHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveFloor))]
        [OpenApiOperation(tags: _tag, Summary = "Save Floor")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveFloorRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveFloor([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-floor")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveFloorHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DeleteFloor))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Floor")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteFloorRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteFloor([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/delete-floor")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteFloorHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetDetailFloor))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Floor")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailFloorRequest.IdFloor), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailFloorResult))]
        public Task<IActionResult> GetDetailFloor([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-floor")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDetailFloorHandler.Execute(req, cancellationToken);
        }
    }
}
