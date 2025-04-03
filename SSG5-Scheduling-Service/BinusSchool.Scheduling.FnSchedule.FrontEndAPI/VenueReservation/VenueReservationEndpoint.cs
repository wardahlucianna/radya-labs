using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs;
using Microsoft.OpenApi.Models;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation
{
    public class VenueReservationEndpoint
    {
        private const string _route = "venue-reservation";
        private const string _tag = "Venue Reservation";

        // Booking Venue and Equipment
        private readonly DeleteVenueReservationBookingHandler _deleteVenueReservationBookingHandler;
        private readonly GetVenueReservationUserLoginSpecialRoleHandler _getVenueReservationUserLoginSpecialRoleHandler;
        private readonly GetVenueReservationBookingListHandler _getVenueReservationBookingListHandler;
        private readonly GetVenueReservationBookingListDetailHandler _getVenueReservationBookingListDetailHandler;
        private readonly GetVenueReservationScheduleTimelineHandler _getVenueReservationScheduleTimelineHandler;
        private readonly GetVenueReservationOverlapCheckHandler _getVenueReservationScheduleOverlapCheckHandler;
        private readonly SaveVenueReservationBookingHandler _saveVenueReservationBookingHandler;
        private readonly UpdateVenueReservationPrepAndCleanTimeHandler _updateVenueReservationPrepAndCleanTimeHandler;
        private readonly UpdateVenueReservationOverlapStatusHandler _updateVenueReservationOverlapStatusHandler;

        //Booking Equipment Only
        private readonly GetListBookingEquipmentOnlyHandler _getListBookingEquipmentOnlyHandler;
        private readonly SaveBookingEquipmentOnlyHandler _saveBookingEquipmentOnlyHandler;
        private readonly GetListEquipmentWithAvailableStockHandler _getListEquipmentWithAvailableStockHandler;
        private readonly GetDetailBookingEquipmentOnlyHandler _getDetailBookingEquipmentOnlyHandler;
        private readonly DeleteBookingEquipmentOnlyHandler _deleteBookingEquipmentOnlyHandler;

        // Venue Reservation Approval
        private readonly GetVenueReservationApprovalListHandler _getVenueReservationApprovalListHandler;
        private readonly GetVenueReservationApprovalDetailHandler _getVenueReservationApprovalDetailHandler;
        private readonly ChangeVenueReservationApprovalStatusHandler _changeVenueReservationApprovalStatusHandler;

        // Venue And Equipment Reservation Summary
        private readonly GetVenueAndEquipmentReservationSummaryHandler _getVenueAndEquipmentReservationSummaryHandler;
        private readonly GetEquipmentReservationSummaryHandler _getEquipmentReservationSummaryHandler;
        private readonly GetVenueReservationOverlappingSummaryHandler _getVenueReservationOverlappingSummaryHandler;
        private readonly GetCurrentAvailableEquipmentStockHandler _getCurrentAvailableEquipmentStockHandler;
        private readonly GetCurrentAvailableVenueHandler _getCurrentAvailableVenueHandler;
        private readonly ExportExcelVenueAndEquipmentReservationSummaryHandler _exportExcelVenueAndEquipmentReservationSummaryHandler;
        private readonly ExportExcelEquipmentReservationSummaryHandler _exportExcelEquipmentReservationSummaryHandler;
        private readonly ExportExcelVenueReservationOverlappingSummaryHandler _exportExcelVenueReservationOverlappingSummaryHandler;
        private readonly ExportExcelCurrentAvailableEquipmentStockHandler _exportExcelCurrentAvailableEquipmentStockHandler;
        private readonly ExportExcelCurrentAvailableVenueHandler _exportExcelCurrentAvailableVenueHandler;

        public VenueReservationEndpoint(
            // Booking Venue and Equipment
            DeleteVenueReservationBookingHandler deleteVenueReservationBookingHandler,
            GetVenueReservationUserLoginSpecialRoleHandler getVenueReservationUserLoginSpecialRoleHandler,
            GetVenueReservationBookingListHandler getVenueReservationBookingListHandler,
            GetVenueReservationBookingListDetailHandler getVenueReservationBookingListDetailHandler,
            GetVenueReservationScheduleTimelineHandler getVenueReservationScheduleTimelineHandler,
            GetVenueReservationOverlapCheckHandler getVenueReservationScheduleOverlapCheckHandler,
            SaveVenueReservationBookingHandler saveVenueReservationBookingHandler,
            UpdateVenueReservationPrepAndCleanTimeHandler updateVenueReservationPrepAndCleanTimeHandler,
            UpdateVenueReservationOverlapStatusHandler updateVenueReservationOverlapStatusHandler,
            
            // Booking Equipment Only
            GetListBookingEquipmentOnlyHandler getListBookingEquipmentOnlyHandler,
            SaveBookingEquipmentOnlyHandler saveBookingEquipmentOnlyHandler,
            GetListEquipmentWithAvailableStockHandler getListEquipmentWithAvailableStockHandler,
            GetDetailBookingEquipmentOnlyHandler getDetailBookingEquipmentOnlyHandler,
            DeleteBookingEquipmentOnlyHandler deleteBookingEquipmentOnlyHandler,

            // Venue Reservation Approval
            GetVenueReservationApprovalListHandler getVenueReservationApprovalListHandler,
            GetVenueReservationApprovalDetailHandler getVenueReservationApprovalDetailHandler,
            ChangeVenueReservationApprovalStatusHandler changeVenueReservationApprovalStatusHandler,
            
            // Venue And Equipment Reservation Summary
            GetVenueAndEquipmentReservationSummaryHandler getVenueAndEquipmentReservationSummaryHandler,
            GetEquipmentReservationSummaryHandler getEquipmentReservationSummaryHandler,
            GetVenueReservationOverlappingSummaryHandler getVenueReservationOverlappingSummaryHandler,
            GetCurrentAvailableEquipmentStockHandler getCurrentAvailableEquipmentStockHandler,
            GetCurrentAvailableVenueHandler getCurrentAvailableVenueHandler,
            ExportExcelVenueAndEquipmentReservationSummaryHandler exportExcelVenueAndEquipmentReservationSummaryHandler,
            ExportExcelEquipmentReservationSummaryHandler exportExcelEquipmentReservationSummaryHandler,
            ExportExcelVenueReservationOverlappingSummaryHandler exportExcelVenueReservationOverlappingSummaryHandler,
            ExportExcelCurrentAvailableEquipmentStockHandler exportExcelCurrentAvailableEquipmentStockHandler,
            ExportExcelCurrentAvailableVenueHandler exportExcelCurrentAvailableVenueHandler)
        {
            // Booking Venue and Equipment
            _deleteVenueReservationBookingHandler = deleteVenueReservationBookingHandler;
            _getVenueReservationUserLoginSpecialRoleHandler = getVenueReservationUserLoginSpecialRoleHandler;
            _getVenueReservationBookingListHandler = getVenueReservationBookingListHandler;
            _getVenueReservationBookingListDetailHandler = getVenueReservationBookingListDetailHandler;
            _getVenueReservationScheduleTimelineHandler = getVenueReservationScheduleTimelineHandler;
            _getVenueReservationScheduleOverlapCheckHandler = getVenueReservationScheduleOverlapCheckHandler;
            _saveVenueReservationBookingHandler = saveVenueReservationBookingHandler;
            _updateVenueReservationPrepAndCleanTimeHandler = updateVenueReservationPrepAndCleanTimeHandler;
            _updateVenueReservationOverlapStatusHandler = updateVenueReservationOverlapStatusHandler;

            // Booking Equipment Only
            _getListBookingEquipmentOnlyHandler = getListBookingEquipmentOnlyHandler;
            _saveBookingEquipmentOnlyHandler = saveBookingEquipmentOnlyHandler;
            _getListEquipmentWithAvailableStockHandler = getListEquipmentWithAvailableStockHandler;
            _getDetailBookingEquipmentOnlyHandler = getDetailBookingEquipmentOnlyHandler;
            _deleteBookingEquipmentOnlyHandler = deleteBookingEquipmentOnlyHandler;

            // Venue Reservation Approval
            _getVenueReservationApprovalListHandler = getVenueReservationApprovalListHandler;
            _getVenueReservationApprovalDetailHandler = getVenueReservationApprovalDetailHandler;
            _changeVenueReservationApprovalStatusHandler = changeVenueReservationApprovalStatusHandler;

            // Venue And Equipment Reservation Summary
            _getVenueAndEquipmentReservationSummaryHandler = getVenueAndEquipmentReservationSummaryHandler;
            _getEquipmentReservationSummaryHandler = getEquipmentReservationSummaryHandler;
            _getVenueReservationOverlappingSummaryHandler = getVenueReservationOverlappingSummaryHandler;
            _getCurrentAvailableEquipmentStockHandler = getCurrentAvailableEquipmentStockHandler;
            _getCurrentAvailableVenueHandler = getCurrentAvailableVenueHandler;
            _exportExcelVenueAndEquipmentReservationSummaryHandler = exportExcelVenueAndEquipmentReservationSummaryHandler;
            _exportExcelEquipmentReservationSummaryHandler = exportExcelEquipmentReservationSummaryHandler;
            _exportExcelVenueReservationOverlappingSummaryHandler = exportExcelVenueReservationOverlappingSummaryHandler;
            _exportExcelCurrentAvailableEquipmentStockHandler = exportExcelCurrentAvailableEquipmentStockHandler;
            _exportExcelCurrentAvailableVenueHandler = exportExcelCurrentAvailableVenueHandler;
        }

        #region Booking Venue and Equipment
        [FunctionName(nameof(VenueReservationEndpoint.DeleteVenueReservationBooking))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Venue Reservation Booking")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteVenueReservationBookingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteVenueReservationBooking(
          [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-venue-reservation-booking")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteVenueReservationBookingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(VenueReservationEndpoint.GetVenueReservationUserLoginSpecialRole))]
        [OpenApiOperation(tags: _tag, Summary = "Get Venue Reservation User Login Special Role")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetVenueReservationUserLoginSpecialRoleRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetVenueReservationUserLoginSpecialRoleResponse))]
        public Task<IActionResult> GetVenueReservationUserLoginSpecialRole(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-venue-reservation-user-login-special-role")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getVenueReservationUserLoginSpecialRoleHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(VenueReservationEndpoint.GetVenueReservationBookingList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Venue Reservation Booking List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetVenueReservationBookingListRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetVenueReservationBookingListRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetVenueReservationBookingListRequest.BookingStartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetVenueReservationBookingListRequest.BookingEndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetVenueReservationBookingListRequest.IdVenue), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueReservationBookingListRequest.BookingStatus), In = ParameterLocation.Query, Type = typeof(VenueApprovalStatus?))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetVenueReservationBookingListResponse>))]
        public Task<IActionResult> GetVenueReservationBookingList(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-venue-reservation-booking-list")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getVenueReservationBookingListHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(VenueReservationEndpoint.GetVenueReservationBookingListDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Venue Reservation Booking List Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetVenueReservationBookingListDetailRequest.IdVenueReservation), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetVenueReservationBookingListDetailRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetVenueReservationBookingListDetailResponse))]
        public Task<IActionResult> GetVenueReservationBookingListDetail(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-venue-reservation-booking-list-detail")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getVenueReservationBookingListDetailHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(VenueReservationEndpoint.GetVenueReservationScheduleTimeline))]
        [OpenApiOperation(tags: _tag, Summary = "Get Venue Reservation Schedule Timeline")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetVenueReservationScheduleTimelineRequest.Date), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetVenueReservationScheduleTimelineRequest.IdFloor), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetVenueReservationScheduleTimelineResponse))]
        public Task<IActionResult> GetVenueReservationScheduleTimeline(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-venue-reservation-schedule-timeline")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getVenueReservationScheduleTimelineHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(VenueReservationEndpoint.GetVenueReservationOverlapCheck))]
        [OpenApiOperation(tags: _tag, Summary = "Get Venue Reservation Overlap Check")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetVenueReservationOverlapCheckRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetVenueReservationOverlapCheckResponse>))]
        public Task<IActionResult> GetVenueReservationOverlapCheck(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-venue-reservation-overlap-check")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getVenueReservationScheduleOverlapCheckHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(VenueReservationEndpoint.SaveVenueReservationBooking))]
        [OpenApiOperation(tags: _tag, Summary = "Save Venue Reservation Booking")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(List<SaveVenueReservationBookingRequest>))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SaveVenueReservationBookingResponse))]
        public Task<IActionResult> SaveVenueReservationBooking(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-venue-reservation-booking")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveVenueReservationBookingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(VenueReservationEndpoint.UpdateVenueReservationPrepAndCleanTime))]
        [OpenApiOperation(tags: _tag, Summary = "Update Venue Reservation Prep And Clean Time")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateVenueReservationPrepAndCleanTimeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateVenueReservationPrepAndCleanTime(
          [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-venue-reservation-prep-and-clean-time")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _updateVenueReservationPrepAndCleanTimeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(VenueReservationEndpoint.UpdateVenueReservationOverlapStatus))]
        [OpenApiOperation(tags: _tag, Summary = "Update Venue Reservation Overlap Status")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateVenueReservationOverlapStatusRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateVenueReservationOverlapStatus(
          [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-venue-reservation-overlap-status")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _updateVenueReservationOverlapStatusHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region Booking Equipment Only
        [FunctionName(nameof(GetListBookingEquipmentOnly))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Booking Equipment Only")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListBookingEquipmentOnlyRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListBookingEquipmentOnlyRequest.StartDate), In = ParameterLocation.Query, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetListBookingEquipmentOnlyRequest.EndDate), In = ParameterLocation.Query, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetListBookingEquipmentOnlyRequest.GetAllData), In = ParameterLocation.Query, Required = true, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListBookingEquipmentOnlyResult>))]
        public Task<IActionResult> GetListBookingEquipmentOnly(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-booking-equipment-only")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getListBookingEquipmentOnlyHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveBookingEquipmentOnly))]
        [OpenApiOperation(tags: _tag, Summary = "Save Booking Equipment Only")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveBookingEquipmentOnlyRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveBookingEquipmentOnly(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-booking-equipment-only")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveBookingEquipmentOnlyHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetListEquipmentWithAvailableStock))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Equipment With Available Stock")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListEquipmentWithAvailableStockRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListEquipmentWithAvailableStockRequest.Date), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetListEquipmentWithAvailableStockRequest.StartTime), In = ParameterLocation.Query, Required = true, Type = typeof(TimeSpan))]
        [OpenApiParameter(nameof(GetListEquipmentWithAvailableStockRequest.EndTime), In = ParameterLocation.Query, Required = true, Type = typeof(TimeSpan))]
        [OpenApiParameter(nameof(GetListEquipmentWithAvailableStockRequest.IdMappingEquipmentReservation), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<ListEquipmentForBookingEquipmentOnly>))]
        public Task<IActionResult> GetListEquipmentWithAvailableStock(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-equipment-with-available-stock")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getListEquipmentWithAvailableStockHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetDetailBookingEquipmentOnly))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Booking Equipment Only")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailBookingEquipmentOnlyRequest.IdMappingEquipmentReservation), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailBookingEquipmentOnlyResult))]
        public Task<IActionResult> GetDetailBookingEquipmentOnly(
                       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-booking-equipment-only")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDetailBookingEquipmentOnlyHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DeleteBookingEquipmentOnly))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Booking Equipment Only")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteBookingEquipmentOnlyRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteBookingEquipmentOnly(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-booking-equipment-only")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteBookingEquipmentOnlyHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region Venue Approval Status
        [FunctionName(nameof(VenueReservationEndpoint.GetVenueReservationApprovalList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Venue Reservation Approval List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetVenueReservationApprovalListRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetVenueReservationApprovalListRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetVenueReservationApprovalListRequest.BookingStartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetVenueReservationApprovalListRequest.BookingEndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetVenueReservationApprovalListRequest.IdVenue), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueReservationApprovalListRequest.BookingStatus), In = ParameterLocation.Query, Type = typeof(VenueApprovalStatus?))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetVenueReservationApprovalListResponse>))]
        public Task<IActionResult> GetVenueReservationApprovalList(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-venue-reservation-approval-list")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getVenueReservationApprovalListHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(VenueReservationEndpoint.GetVenueReservationApprovalDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Venue Reservation Approval Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetVenueReservationApprovalDetailRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetVenueReservationApprovalDetailRequest.IdBooking), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetVenueReservationApprovalDetailResponse))]
        public Task<IActionResult> GetVenueReservationApprovalDetail(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-venue-reservation-approval-detail")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getVenueReservationApprovalDetailHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(VenueReservationEndpoint.ChangeVenueReservationApprovalStatus))]
        [OpenApiOperation(tags: _tag, Summary = "Change Venue Reservation Approval Status")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(List<ChangeVenueReservationApprovalStatusRequest>))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ChangeVenueReservationApprovalStatus(
          [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/change-venue-reservation-approval-status")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _changeVenueReservationApprovalStatusHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region Venue And Equipment Reservation Summary
        [FunctionName(nameof(VenueReservationEndpoint.GetVenueAndEquipmentReservationSummary))]
        [OpenApiOperation(tags: _tag, Summary = "Get Venue And Equipment Reservation Summary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetVenueAndEquipmentReservationSummaryRequest.BookingStartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetVenueAndEquipmentReservationSummaryRequest.BookingEndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetVenueAndEquipmentReservationSummaryRequest.IdBuilding), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueAndEquipmentReservationSummaryRequest.IdVenue), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueAndEquipmentReservationSummaryRequest.ApprovalStatus), In = ParameterLocation.Query, Type = typeof(VenueApprovalStatus?))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetVenueAndEquipmentReservationSummaryResponse>))]
        public Task<IActionResult> GetVenueAndEquipmentReservationSummary(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-venue-and-equipment-reservation-summary")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getVenueAndEquipmentReservationSummaryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(VenueReservationEndpoint.GetEquipmentReservationSummary))]
        [OpenApiOperation(tags: _tag, Summary = "Get Equipment Reservation Summary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetEquipmentReservationSummaryRequest.BookingStartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetEquipmentReservationSummaryRequest.BookingEndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetEquipmentReservationSummaryRequest.IdEquipmentType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEquipmentReservationSummaryRequest.IdEquipment), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetEquipmentReservationSummaryResponse>))]
        public Task<IActionResult> GetEquipmentReservationSummary(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-equipment-reservation-summary")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getEquipmentReservationSummaryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(VenueReservationEndpoint.GetVenueReservationOverlappingSummary))]
        [OpenApiOperation(tags: _tag, Summary = "Get Venue Reservation Overlapping Summary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetVenueReservationOverlappingSummaryRequest.BookingStartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetVenueReservationOverlappingSummaryRequest.BookingEndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetVenueReservationOverlappingSummaryRequest.IdBuilding), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueReservationOverlappingSummaryRequest.IdVenue), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueReservationOverlappingSummaryRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueReservationOverlappingSummaryRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueReservationOverlappingSummaryRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetVenueReservationOverlappingSummaryRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetVenueReservationOverlappingSummaryRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueReservationOverlappingSummaryRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueReservationOverlappingSummaryRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetVenueReservationOverlappingSummaryResponse>))]
        public Task<IActionResult> GetVenueReservationOverlappingSummary(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-venue-reservation-overlapping-summary")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getVenueReservationOverlappingSummaryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetCurrentAvailableEquipmentStock))]
        [OpenApiOperation(tags: _tag, Summary = "Get Current Available Equipment Stock")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCurrentAvailableEquipmentStockRequest.BookingStartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetCurrentAvailableEquipmentStockRequest.BookingEndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetCurrentAvailableEquipmentStockRequest.IdEquipmentType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCurrentAvailableEquipmentStockRequest.StartTime), In = ParameterLocation.Query, Required = true, Type = typeof(TimeSpan))]
        [OpenApiParameter(nameof(GetCurrentAvailableEquipmentStockRequest.EndTime), In = ParameterLocation.Query, Required = true, Type = typeof(TimeSpan))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetCurrentAvailableEquipmentStockResult>))]
        public Task<IActionResult> GetCurrentAvailableEquipmentStock(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-current-available-equipment-stock")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getCurrentAvailableEquipmentStockHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetCurrentAvailableVenue))]
        [OpenApiOperation(tags: _tag, Summary = "Get Current Available Venue")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCurrentAvailableVenueRequest.BookingStartDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetCurrentAvailableVenueRequest.BookingEndDate), In = ParameterLocation.Query, Required = true, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetCurrentAvailableVenueRequest.IdVenue), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCurrentAvailableVenueRequest.IdBuilding), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCurrentAvailableVenueRequest.StartTime), In = ParameterLocation.Query, Required = true, Type = typeof(TimeSpan))]
        [OpenApiParameter(nameof(GetCurrentAvailableVenueRequest.EndTime), In = ParameterLocation.Query, Required = true, Type = typeof(TimeSpan))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetCurrentAvailableVenueResult>))]
        public Task<IActionResult> GetCurrentAvailableVenue(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-current-available-venue")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getCurrentAvailableVenueHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(VenueReservationEndpoint.ExportExcelVenueAndEquipmentReservationSummary))]
        [OpenApiOperation(tags: _tag, Summary = "Export Excel Venue And Equipment Reservation Summary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportExcelVenueAndEquipmentReservationSummaryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelVenueAndEquipmentReservationSummary(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/export-excel-venue-and-equipment-reservation-summary")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _exportExcelVenueAndEquipmentReservationSummaryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(VenueReservationEndpoint.ExportExcelEquipmentReservationSummary))]
        [OpenApiOperation(tags: _tag, Summary = "Expor Excel Equipment Reservation Summary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportExcelEquipmentReservationSummaryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelEquipmentReservationSummary(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/export-excel-equipment-reservation-summary")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _exportExcelEquipmentReservationSummaryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(VenueReservationEndpoint.ExportExcelVenueReservationOverlappingSummary))]
        [OpenApiOperation(tags: _tag, Summary = "Expor Excel Venue Reservation Overlapping Summary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetVenueReservationOverlappingSummaryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelVenueReservationOverlappingSummary(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/export-excel-venue-reservation-overlapping-summary")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _exportExcelVenueReservationOverlappingSummaryHandler.Execute(req, cancellationToken);
        }
        [FunctionName(nameof(ExportExcelCurrentAvailableEquipmentStock))]
        [OpenApiOperation(tags: _tag, Summary = "Export Excel Current Available Equipment Stock")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportExcelCurrentAvailableEquipmentStockRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelCurrentAvailableEquipmentStock(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/export-excel-current-available-equipment-stock")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _exportExcelCurrentAvailableEquipmentStockHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExportExcelCurrentAvailableVenue))]
        [OpenApiOperation(tags: _tag, Summary = "Export Excel Current Available Venue")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportExcelCurrentAvailableVenueRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelCurrentAvailableVenue(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/export-excel-current-available-venue")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _exportExcelCurrentAvailableVenueHandler.Execute(req, cancellationToken);
        }
        #endregion
    }
}
