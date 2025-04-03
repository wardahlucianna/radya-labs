using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.School.FnSchool.MasterVenueReservationRule;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.MasterVenueReservationRule
{
    public class MasterVenueReservationRuleEndpoint
    {
        private const string _route = "school/master-venue-reservation-rule";
        private const string _tag = "Master Venue Reservation Rule";

        private readonly GetMasterVenueReservationRuleHandler _getMasterVenueReservationRuleHandler;
        private readonly SaveMasterVenueReservationRuleHandler _saveMasterVenueReservationRuleHandler;

        public MasterVenueReservationRuleEndpoint(
            GetMasterVenueReservationRuleHandler getMasterVenueReservationRuleHandler, 
            SaveMasterVenueReservationRuleHandler saveMasterVenueReservationRuleHandler)
        {
            _getMasterVenueReservationRuleHandler = getMasterVenueReservationRuleHandler;
            _saveMasterVenueReservationRuleHandler = saveMasterVenueReservationRuleHandler;
        }

        [FunctionName(nameof(GetMasterVenueReservationRule))]
        [OpenApiOperation(tags: _tag, Summary = "Get Master Venue Reservation Rule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMasterVenueReservationRuleRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMasterVenueReservationRuleResult))]

        public Task<IActionResult> GetMasterVenueReservationRule(
               [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-master-venue-reservation-rule")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getMasterVenueReservationRuleHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveMasterVenueReservationRule))]
        [OpenApiOperation(tags: _tag, Summary = "Save Master Venue Reservation Rule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveMasterVenueReservationRuleRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]

        public Task<IActionResult> SaveMasterVenueReservationRule(
                       [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-master-venue-reservation-rule")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveMasterVenueReservationRuleHandler.Execute(req, cancellationToken);
        }
    }
}
