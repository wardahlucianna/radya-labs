using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.Venue;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.Venue
{
    public class VenueEndpoint
    {
        private const string _route = "school/venue";
        private const string _tag = "School Venue";

        private readonly VenueHandler _venueHandler;
        private readonly GetVenueForAscTimetableHandler _getVenueForAscTimetable;
        public VenueEndpoint(VenueHandler venueHandler,
            GetVenueForAscTimetableHandler getVenueForAscTimetable)
        {
            _venueHandler = venueHandler;
            _getVenueForAscTimetable = getVenueForAscTimetable;
        }

        [FunctionName(nameof(VenueEndpoint.GetVenueslitsAsctimetable))]
        [OpenApiOperation(tags: _tag, Summary = "Get Venue List for asc time table")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetVenueForAscTimetableRequest.VenueCode), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetVenueForAscTimetableRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetVenueResult))]
        public Task<IActionResult> GetVenueslitsAsctimetable(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route+"-by-code")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getVenueForAscTimetable.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(VenueEndpoint.GetVenues))]
        [OpenApiOperation(tags: _tag, Summary = "Get Venue List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetVenueRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetVenueRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetVenueRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetVenueRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetVenueRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueRequest.IdBuilding), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetVenueResult))]
        public Task<IActionResult> GetVenues(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _venueHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(VenueEndpoint.GetVenueDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Venue Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetVenueDetailResult))]
        public Task<IActionResult> GetVenueDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            return _venueHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(VenueEndpoint.AddVenue))]
        [OpenApiOperation(tags: _tag, Summary = "Add Venue")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddVenueRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddVenue(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _venueHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(VenueEndpoint.UpdateVenue))]
        [OpenApiOperation(tags: _tag, Summary = "Update Venue")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateVenueRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateVenue(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _venueHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(VenueEndpoint.DeleteVenue))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Venue")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteVenue(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _venueHandler.Execute(req, cancellationToken);
        }
    }
}
