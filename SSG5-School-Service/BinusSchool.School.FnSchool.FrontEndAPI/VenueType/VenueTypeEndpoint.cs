using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.VenueType;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.VenueType
{
    public class VenueTypeEndpoint
    {
        private const string _route = "school/venue-type";
        private const string _tag = "School Venue Type";

        private readonly GetDDLVenueTypeHandler _getDDLVenueTypeHandler;
        private readonly GetListVenueTypeHandler _getListVenueTypeHandler;
        private readonly SaveVenueTypeHandler _saveVenueTypeHandler;
        private readonly DeleteVenueTypeHandler _deleteVenueTypeHandler;
        private readonly GetDetailVenueTypeHandler _getDetailVenueTypeHandler;

        public VenueTypeEndpoint(
            GetDDLVenueTypeHandler getDDLVenueTypeHandler, 
            GetListVenueTypeHandler getListVenueTypeHandler, 
            SaveVenueTypeHandler saveVenueTypeHandler, 
            DeleteVenueTypeHandler deleteVenueTypeHandler,
            GetDetailVenueTypeHandler getDetailVenueTypeHandler)
        {
            _getDDLVenueTypeHandler = getDDLVenueTypeHandler;
            _getListVenueTypeHandler = getListVenueTypeHandler;
            _saveVenueTypeHandler = saveVenueTypeHandler;
            _deleteVenueTypeHandler = deleteVenueTypeHandler;
            _getDetailVenueTypeHandler = getDetailVenueTypeHandler;
        }

        [FunctionName(nameof(GetDDLVenueType))]
        [OpenApiOperation(tags: _tag, Summary = "Get DDL Venue Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDDLVenueTypeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<ItemValueVm>))]


        public Task<IActionResult> GetDDLVenueType([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-ddl-venue-type")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDDLVenueTypeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetListVenueType))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Venue Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListVenueTypeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListVenueTypeResult>))]
        public Task<IActionResult> GetListVenueType([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-venue-type")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getListVenueTypeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveVenueType))]
        [OpenApiOperation(tags: _tag, Summary = "Save Venue Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveVenueTypeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveVenueType([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-venue-type")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveVenueTypeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DeleteVenueType))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Venue Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteVenueTypeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteVenueType([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/delete-venue-type")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteVenueTypeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetDetailVenueType))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Venue Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailVenueTypeRequest.IdVenueType), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailVenueTypeResult))]
        public Task<IActionResult> GetDetailVenueType([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-venue-type")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDetailVenueTypeHandler.Execute(req, cancellationToken);
        }
    }
}
