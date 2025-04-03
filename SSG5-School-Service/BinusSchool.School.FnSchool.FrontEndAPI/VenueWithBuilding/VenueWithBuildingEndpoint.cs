using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.VenueWithBuilding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.VenueWithBuilding
{
    public class VenueWithBuildingEndpoint
    {
        private const string _route = "school/venue-with-building";
        private const string _tag = "School Venue With Building";

        private readonly GetVenueWithBuildingHandler _getVenueWithBuildingHandler;
        public VenueWithBuildingEndpoint(GetVenueWithBuildingHandler getVenueWithBuildingHandler)
        {
            _getVenueWithBuildingHandler = getVenueWithBuildingHandler;
        }

        [FunctionName(nameof(VenueWithBuildingEndpoint.GetVenuesWithBuilding))]
        [OpenApiOperation(tags: _tag, Summary = "Get Venue List with Building")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetVenueWithBuildingRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetVenueWithBuildingRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueWithBuildingRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueWithBuildingRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetVenueWithBuildingRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetVenueWithBuildingRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueWithBuildingRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueWithBuildingRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetVenueWithBuildingRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetVenueWithBuildingRequest.IdBuilding), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<ItemValueVm>))]
        public Task<IActionResult> GetVenuesWithBuilding(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getVenueWithBuildingHandler.Execute(req, cancellationToken);
        }
    }
}
