using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using Refit;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary;
using System.Net.Http;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IVenueReservation : IFnSchedule
    {
        #region Booking Venue and Equipment
        [Delete("/venue-reservation/delete-venue-reservation-booking")]
        Task<ApiErrorResult> DeleteVenueReservationBooking([Body] DeleteVenueReservationBookingRequest request);

        [Get("/venue-reservation/get-venue-reservation-user-login-special-role")]
        Task<ApiErrorResult<GetVenueReservationUserLoginSpecialRoleResponse>> GetVenueReservationUserLoginSpecialRole(GetVenueReservationUserLoginSpecialRoleRequest request);

        [Get("/venue-reservation/get-venue-reservation-booking-list")]
        Task<ApiErrorResult<IEnumerable<GetVenueReservationBookingListResponse>>> GetVenueReservationBookingList(GetVenueReservationBookingListRequest request);

        [Get("/venue-reservation/get-venue-reservation-booking-list-detail")]
        Task<ApiErrorResult<GetVenueReservationBookingListDetailResponse>> GetVenueReservationBookingListDetail(GetVenueReservationBookingListDetailRequest request);

        [Get("/venue-reservation/get-venue-reservation-schedule-timeline")]
        Task<ApiErrorResult<GetVenueReservationScheduleTimelineResponse>> GetVenueReservationScheduleTimeline(GetVenueReservationScheduleTimelineRequest request);

        [Post("/venue-reservation/get-venue-reservation-overlap-check")]
        Task<ApiErrorResult<GetVenueReservationOverlapCheckResponse>> GetVenueReservationOverlapCheck([Body] GetVenueReservationOverlapCheckRequest request);

        [Post("/venue-reservation/save-venue-reservation-booking")]
        Task<ApiErrorResult<SaveVenueReservationBookingResponse>> SaveVenueReservationBooking([Body] IEnumerable<SaveVenueReservationBookingRequest> request);

        [Put("/venue-reservation/update-venue-reservation-prep-and-clean-time")]
        Task<ApiErrorResult> UpdateVenueReservationPrepAndCleanTime([Body] UpdateVenueReservationPrepAndCleanTimeRequest request);

        [Put("/venue-reservation/update-venue-reservation-overlap-status")]
        Task<ApiErrorResult> UpdateVenueReservationOverlapStatus([Body] UpdateVenueReservationOverlapStatusRequest request);
        #endregion

        #region Booking Equipment Only
        [Get("/venue-reservation/get-list-booking-equipment-only")]
        Task<ApiErrorResult<IEnumerable<GetListBookingEquipmentOnlyResult>>> GetListBookingEquipmentOnly(GetListBookingEquipmentOnlyRequest request);

        [Post("/venue-reservation/save-booking-equipment-only")]
        Task<ApiErrorResult> SaveBookingEquipmentOnly([Body] SaveBookingEquipmentOnlyRequest request);

        [Get("/venue-reservation/get-list-equipment-with-available-stock")]
        Task<ApiErrorResult<IEnumerable<ListEquipmentForBookingEquipmentOnly>>> GetListEquipmentWithAvailableStock(GetListEquipmentWithAvailableStockRequest request);

        [Get("/venue-reservation/get-detail-booking-equipment-only")]
        Task<ApiErrorResult<GetDetailBookingEquipmentOnlyResult>> GetDetailBookingEquipmentOnly(GetDetailBookingEquipmentOnlyRequest request);

        [Delete("/venue-reservation/delete-booking-equipment-only")]
        Task<ApiErrorResult> DeleteBookingEquipmentOnly([Body] DeleteBookingEquipmentOnlyRequest request);
        #endregion

        #region Venue Reservation Approval
        [Get("/venue-reservation/get-venue-reservation-approval-list")]
        Task<ApiErrorResult<IEnumerable<GetVenueReservationApprovalListResponse>>> GetVenueReservationApprovalList(GetVenueReservationApprovalListRequest request);

        [Get("/venue-reservation/get-venue-reservation-approval-detail")]
        Task<ApiErrorResult<GetVenueReservationApprovalDetailResponse>> GetVenueReservationApprovalDetail(GetVenueReservationApprovalDetailRequest request);

        [Put("/venue-reservation/change-venue-reservation-approval-status")]
        Task<ApiErrorResult> ChangeVenueReservationApprovalStatus([Body] List<ChangeVenueReservationApprovalStatusRequest> request);
        #endregion

        #region Venue And Equipment Reservation Summary
        [Get("/venue-reservation/get-venue-and-equipment-reservation-summary")]
        Task<ApiErrorResult<IEnumerable<GetVenueAndEquipmentReservationSummaryResponse>>> GetVenueAndEquipmentReservationSummary(GetVenueAndEquipmentReservationSummaryRequest request);

        [Get("/venue-reservation/get-equipment-reservation-summary")]
        Task<ApiErrorResult<IEnumerable<GetEquipmentReservationSummaryResponse>>> GetEquipmentReservationSummary(GetEquipmentReservationSummaryRequest request);

        [Get("/venue-reservation/get-venue-reservation-overlapping-summary")]
        Task<ApiErrorResult<IEnumerable<GetVenueReservationOverlappingSummaryResponse>>> GetVenueReservationOverlappingSummary(GetVenueReservationOverlappingSummaryRequest request);

        [Get("/venue-reservation/get-current-available-equipment-stock")]
        Task<ApiErrorResult<IEnumerable<GetCurrentAvailableEquipmentStockResult>>> GetCurrentAvailableEquipmentStock(GetCurrentAvailableEquipmentStockRequest param);

        [Get("/venue-reservation/get-current-available-venue")]
        Task<ApiErrorResult<IEnumerable<GetCurrentAvailableVenueResult>>> GetCurrentAvailableVenue(GetCurrentAvailableVenueRequest param);

        [Post("/venue-reservation/export-excel-venue-and-equipment-reservation-summary")]
        Task<HttpResponseMessage> ExportExcelVenueAndEquipmentReservationSummary([Body] ExportExcelVenueAndEquipmentReservationSummaryRequest request);

        [Post("/venue-reservation/export-excel-equipment-reservation-summary")]
        Task<HttpResponseMessage> ExportExcelEquipmentReservationSummary([Body] ExportExcelEquipmentReservationSummaryRequest request);

        [Post("/venue-reservation/export-excel-venue-reservation-overlapping-summary")]
        Task<HttpResponseMessage> ExportExcelVenueReservationOverlappingSummary([Body] GetVenueReservationOverlappingSummaryRequest request);

        [Post("/venue-reservation/export-excel-current-available-equipment-stock")]
        Task<HttpResponseMessage> ExportExcelCurrentAvailableEquipmentStock([Body] ExportExcelCurrentAvailableEquipmentStockRequest request);

        [Post("/venue-reservation/export-excel-current-available-venue")]
        Task<HttpResponseMessage> ExportExcelCurrentAvailableVenue([Body] ExportExcelCurrentAvailableVenueRequest request);
        #endregion
    }
}
