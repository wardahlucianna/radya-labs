using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Scheduling.FnSchedule.VenueReservationSettings.MasterSpecialRoleVenueReservation;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservationSettings.MasterSpecialRoleVenueReservation;
using BinusSchool.Scheduling.FnSchedule.VenueReservationSettings.MasterDayRestriction;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservationSettings.MasterDayRestriction;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservationSettings
{
    public class VenueReservationSettingsEndpoint
    {
        private const string _route = "schedule/venue-reservation-settings";
        private const string _tag = "Venue Reservation Settings";

        private readonly GetListSpecialRoleVenueHandler _getListSpecialRoleVenueHandler;
        private readonly GetDetailSpecialRoleVenueHandler _getDetailSpecialRoleVenueHandler;
        private readonly DeleteSpecialRoleVenueHandler _deleteSpecialRoleVenueHandler;
        private readonly SaveSpecialRoleVenueHandler _saveSpecialRoleVenueHandler;

        private readonly GetListMasterDayRestrictionHandler _getListMasterDayRestrictionHandler;
        private readonly GetDetailMasterDayRestrictionHandler _getDetailMasterDayRestrictionHandler;
        private readonly DeleteMasterDayRestrictionHandler _deleteMasterDayRestrictionHandler;
        private readonly SaveMasterDayRestrictionHandler _saveMasterDayRestrictionHandler;

        public VenueReservationSettingsEndpoint(
            GetListSpecialRoleVenueHandler getListSpecialRoleVenueHandler,
            GetDetailSpecialRoleVenueHandler  getDetailSpecialRoleVenueHandler,
            DeleteSpecialRoleVenueHandler deleteSpecialRoleVenueHandler,
            SaveSpecialRoleVenueHandler saveSpecialRoleVenueHandler,
            GetListMasterDayRestrictionHandler getListMasterDayRestrictionHandler,
            GetDetailMasterDayRestrictionHandler getDetailMasterDayRestrictionHandler,
            DeleteMasterDayRestrictionHandler deleteMasterDayRestrictionHandler,
            SaveMasterDayRestrictionHandler saveMasterDayRestrictionHandler)
        {
            _getListSpecialRoleVenueHandler = getListSpecialRoleVenueHandler;
            _getDetailSpecialRoleVenueHandler = getDetailSpecialRoleVenueHandler;
            _deleteSpecialRoleVenueHandler = deleteSpecialRoleVenueHandler;
            _saveSpecialRoleVenueHandler = saveSpecialRoleVenueHandler;
            _getListMasterDayRestrictionHandler = getListMasterDayRestrictionHandler;
            _getDetailMasterDayRestrictionHandler = getDetailMasterDayRestrictionHandler;
            _deleteMasterDayRestrictionHandler = deleteMasterDayRestrictionHandler;
            _saveMasterDayRestrictionHandler = saveMasterDayRestrictionHandler;
        }

        #region Master Special Role Venue Reservation

        [FunctionName(nameof(GetlistSpecialRoleVenue))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Special Role Venue")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListSpecialRoleVenueRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetListSpecialRoleVenueRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListSpecialRoleVenueRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListSpecialRoleVenueRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListSpecialRoleVenueRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListSpecialRoleVenueRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListSpecialRoleVenueRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListSpecialRoleVenueRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListSpecialRoleVenueRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListSpecialRoleVenueResult>))]
        public Task<IActionResult> GetlistSpecialRoleVenue(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-special-role-venue")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getListSpecialRoleVenueHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetDetailSpecialRoleVenue))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Special Role Venue")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization" , In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailSpecialRoleVenueRequest.IdSpecialRoleVenue), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailSpecialRoleVenueResult))]
        public Task<IActionResult> GetDetailSpecialRoleVenue(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-special-role-venue")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDetailSpecialRoleVenueHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DeleteSpecialRoleVenue))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Special Role Venue")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization" , In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteSpecialRoleVenueRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteSpecialRoleVenue(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/delete-special-role-venue")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteSpecialRoleVenueHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveSpecialRoleVenue))]
        [OpenApiOperation(tags: _tag, Summary = "Save Special Role Venue")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization" , In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveSpecialRoleVenueRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveSpecialRoleVenue(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-special-role-venue")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveSpecialRoleVenueHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region Master Day Restriction

        [FunctionName(nameof(GetListMasterDayRestriction))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Master Day Restriction")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization" , In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListSpecialRoleVenueRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetListSpecialRoleVenueRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListSpecialRoleVenueRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListSpecialRoleVenueRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListSpecialRoleVenueRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListSpecialRoleVenueRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListSpecialRoleVenueRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListSpecialRoleVenueRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListMasterDayRestrictionRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListMasterDayRestrictionResult>))]

        public Task<IActionResult> GetListMasterDayRestriction(
                       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-master-day-restriction")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getListMasterDayRestrictionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetDetailMasterDayRestriction))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Master Day Restriction")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization" , In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailMasterDayRestrictionRequest.IdGroupRestriction), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailMasterDayRestrictionResult))]

        public Task<IActionResult> GetDetailMasterDayRestriction(
                        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-master-day-restriction")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDetailMasterDayRestrictionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DeleteMasterDayRestriction))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Master Day Restriction")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization" , In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteMasterDayRestrictionRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]

        public Task<IActionResult> DeleteMasterDayRestriction(
                        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/delete-master-day-restriction")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteMasterDayRestrictionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveMasterDayRestriction))]
        [OpenApiOperation(tags: _tag, Summary = "Save Master Day Restriction")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization" , In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveMasterDayRestrictionRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]

        public Task<IActionResult> SaveMasterDayRestriction(
                        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-master-day-restriction")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveMasterDayRestrictionHandler.Execute(req, cancellationToken);
        }

        #endregion
    }
}
