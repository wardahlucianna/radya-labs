using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.Floor;
using BinusSchool.Data.Model.School.FnSchool.VenueReservationOwner;
using BinusSchool.School.FnSchool.ClassRoomMapping;
using BinusSchool.School.FnSchool.Floor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.VenueReservationOwner
{
    public class VenueReservationOwnerEndpoint
    {
        private const string _route = "school/venue-reservation-owner";
        private const string _tag = "School Venue Reservation Owner";

        private readonly GetDDLVenueReservationOwnerHandler _getDDLVenueReservationOwnerHandler;
        private readonly DeletePICHandler _deletePICHandler;
        private readonly GetDetailPICHandler _getDetailPICHandler;
        private readonly GetListPICHandler _getListPICHandler;
        private readonly SavePICOwnerHandler _savePICOwnerHandler;

        public VenueReservationOwnerEndpoint
        (
            GetDDLVenueReservationOwnerHandler getDDLVenueReservationOwnerHandler,
            DeletePICHandler deletePICHandler,
            GetDetailPICHandler getDetailPICHandler,
            GetListPICHandler getListPICHandler,
            SavePICOwnerHandler savePICOwnerHandler
        )
        {
            _getDDLVenueReservationOwnerHandler = getDDLVenueReservationOwnerHandler;
            _deletePICHandler = deletePICHandler;
            _getDetailPICHandler = getDetailPICHandler;
            _getListPICHandler = getListPICHandler;
            _savePICOwnerHandler = savePICOwnerHandler;
        }

        [FunctionName(nameof(GetDDLVenueReservationOwner))]
        [OpenApiOperation(tags: _tag, Summary = "Get DDL Venue Reservation Owner")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDDLVenueReservationOwnerRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDDLVenueReservationOwnerRequest.IsPICVenue), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetDDLVenueReservationOwnerRequest.IsPICEquipment), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<NameValueVm>))]

        public Task<IActionResult> GetDDLVenueReservationOwner(
                               [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-ddl-venue-reservation-owner")] HttpRequest req,
                                                          CancellationToken cancellationToken)
        {
            return _getDDLVenueReservationOwnerHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetListPIC))]
        [OpenApiOperation(tags: _tag, Summary = "Get List PIC")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListPICRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListPICResult>))]
        public Task<IActionResult> GetListPIC([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-pic")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getListPICHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SavePICOwner))]
        [OpenApiOperation(tags: _tag, Summary = "Save PIC Owner")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SavePicOwnerRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SavePICOwner([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-pic-owner")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _savePICOwnerHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetDetailPIC))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail PIC")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailPICRequest.IdReservationOwner), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailPICResult))]
        public Task<IActionResult> GetDetailPIC([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-pic")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDetailPICHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DeletePIC))]
        [OpenApiOperation(tags: _tag, Summary = "Delete PIC Owner")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeletePICOwnerRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeletePIC([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/delete-pic")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deletePICHandler.Execute(req, cancellationToken); 
        }
    }
}
