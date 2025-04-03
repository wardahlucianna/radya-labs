using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.VenueEquipment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.VenueEquipment
{
    public class VenueEquipmentEndpoint
    {
        private const string _route = "school/venue-equipment";
        private const string _tag = "School Venue Equipment";

        private readonly GetListVenueEquipmentHandler _getListVenueEquipmentHandler;
        private readonly GetListSelectEquipmentHandler _getListSelectEquipmentHandler;
        private readonly GetVenueEquipmentDetailHandler _getVenueEquipmentDetailHandler;
        private readonly DeleteVenueEquipmentHandler _deleteVenueEquipmentHandler;
        private readonly SaveVenueEquipmentHandler _saveVenueEquipmentHandler;

        public VenueEquipmentEndpoint
        (
        GetListVenueEquipmentHandler getListVenueEquipmentHandler
,       GetListSelectEquipmentHandler getListSelectEquipmentHandler,
        GetVenueEquipmentDetailHandler getVenueEquipmentDetailHandler,
        DeleteVenueEquipmentHandler deleteVenueEquipmentHandler,
        SaveVenueEquipmentHandler saveVenueEquipmentHandler
        )
        {
            _getListVenueEquipmentHandler = getListVenueEquipmentHandler;
            _getListSelectEquipmentHandler = getListSelectEquipmentHandler;
            _getVenueEquipmentDetailHandler = getVenueEquipmentDetailHandler;
            _deleteVenueEquipmentHandler = deleteVenueEquipmentHandler;
            _saveVenueEquipmentHandler = saveVenueEquipmentHandler;
        }

        [FunctionName(nameof(GetListVenueEquipment))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Venue Equipment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListVenueEquipmentRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListVenueEquipmentRequest.Venue), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListVenueEquipmentRequest.Building), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListVenueEquipmentRequest.SearchKey), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListVenueEquipmentRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListVenueEquipmentResult>))]
        public Task<IActionResult> GetListVenueEquipment([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-venue-equipment")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getListVenueEquipmentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetListSelectEquipment))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Select Equipment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListSelectEquipmentRequest.EquipmentType), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetListSelectEquipmentRequest.IdSchool), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListSelectEquipmentResult>))]
        public Task<IActionResult> GetListSelectEquipment([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-select-equipment")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getListSelectEquipmentHandler.Execute(req, cancellationToken); 
        }

        [FunctionName(nameof(SaveVenueEquipment))]
        [OpenApiOperation(tags: _tag, Summary = "Save Venue Equipment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveVenueEquipRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveVenueEquipment([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-venue-equipment")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveVenueEquipmentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetVenueEquipmentDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Venue Equipment Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetVenueEquipmentDetailRequest.IdVenue), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetVenueEquipmentDetailResult))]
        public Task<IActionResult> GetVenueEquipmentDetail([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-venue-equipment-detail")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getVenueEquipmentDetailHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DeleteVenueEquipment))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Venue Equipment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteVenueEquipmentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteVenueEquipment([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/delete-venue-equipment")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteVenueEquipmentHandler.Execute(req, cancellationToken);
        }
    }
}
