using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IAppointmentBooking : IFnSchedule
    {
        #region Appointment Booking Setting
        [Get("/schedule/appointment-booking/get-event-parent-and-student")]
        Task<ApiErrorResult<IEnumerable<GetEventParentAndStudentResult>>> GetEventParentAndStudent(GetEventParentAndStudentRequest query);

        [Get("/schedule/appointment-booking/get-list-invitation-booking-setting")]
        Task<ApiErrorResult<IEnumerable<GetListInvitationBookingSettingResult>>> GetListInvitationBookingSetting(GetListInvitationBookingSettingRequest query);

        [Post("/schedule/appointment-booking/create-invitation-booking-setting")]
        Task<ApiErrorResult> CreateInvitationBookingSetting([Body] CreateInvitationBookingSettingRequest query);

        [Get("/schedule/appointment-booking/detail-invitation-booking-setting")]
        Task<ApiErrorResult<DetailInvitationBookingSettingResult>> DetailInvitationBookingSetting(DetailInvitationBookingSettingRequest query);
        
        [Delete("/schedule/appointment-booking/delete-invitation-booking-setting")]
        Task<ApiErrorResult> DeleteInvitationBookingSetting([Body] DeleteInvitationBookingSettingRequest body);

        [Get("/schedule/appointment-booking/get-grade-by-appointment-date")]
        Task<ApiErrorResult<IEnumerable<GetGradeByAppointmentDateResult>>> GetGradeByAppointmentDate(GetGradeByAppointmentDateRequest query);


        [Get("/schedule/appointment-booking/get-list-break-setting")]
        Task<ApiErrorResult<IEnumerable<GetListBreakSettingResult>>> GetListBreakSetting(GetListBreakSettingRequest query);

        [Post("/schedule/appointment-booking/add-break-setting")]
        Task<ApiErrorResult> AddBreakSetting([Body] AddBreakSettingRequest query);

        [Get("/schedule/appointment-booking/detail-break-setting")]
        Task<ApiErrorResult<DetailBreakSettingResult>> DetailBreakSetting(DetailBreakSettingRequest query);

        [Put("/schedule/appointment-booking/update-break-setting")]
        Task<ApiErrorResult> UpdateBreakSetting([Body] UpdateBreakSettingRequest body);

        [Delete("/schedule/appointment-booking/delete-break-setting")]
        Task<ApiErrorResult> DeleteBreakSetting([Body] DeleteBreakSettingRequest body);

        [Get("/schedule/appointment-booking/get-list-schedule-preview")]
        Task<ApiErrorResult<IEnumerable<GetSchedulePreviewResult>>> GetListSchedulePreview(GetSchedulePreviewRequest query);

        [Get("/schedule/appointment-booking/get-list-recap")]
        Task<ApiErrorResult<IEnumerable<GetListRecapResult>>> GetListRecap(GetListRecapRequest query);

        [Get("/schedule/appointment-booking/download-list-recap")]
        Task<HttpResponseMessage> DownloadListRecap(DownloadListRecapRequest query);

        [Get("/schedule/appointment-booking/get-list-teacher-by-invitation")]
        Task<ApiErrorResult<IEnumerable<GetListTeacherByInvitationResult>>> GetListTeacherByInvitation(GetListTeacherByInvitationRequest query);

        [Get("/schedule/appointment-booking/get-list-time-from-quota-duration")]
        Task<ApiErrorResult<IEnumerable<GetListTimeFromQuotaDurationResult>>> GetListTimeFromQuotaDuration(GetListTimeFromQuotaDurationRequest query);
        
        [Get("/schedule/appointment-booking/get-user-for-user-venue")]
        Task<ApiErrorResult<IEnumerable<GetUserForUserVenueResult>>> GetUserForUserVenue(GetUserForUserVenueRequest query);

        [Post("/schedule/appointment-booking/update-vanue")]
        Task<ApiErrorResult> UpdateInvitationBookingSettingVanueOnly([Body] UpdateInvitationBookingSettingVanueOnlyRequest query);
        [Post("/schedule/appointment-booking/get-user-by-role-position-exclude-subject")]
        Task<ApiErrorResult<IEnumerable<GetUserByRolePositionExcludeSubjectResult>>> GetUserByRolePositionExcludeSubject(GetUserByRolePositionExcludeSubjectRequest query);

        #endregion

        #region Appoitment-booking-parent
        [Get("/schedule/appointment-booking-parent")]
        Task<ApiErrorResult<IEnumerable<GetAppointmentBookingParentResult>>> GetAppointmentBookingParent(GetAppointmentBookingParentRequest query);

        [Post("/schedule/appointment-booking-parent")]
        Task<ApiErrorResult> AddAppointmentBookingParent([Body] AddAppointmentBookingParentRequest body);

        [Get("/schedule/appointment-booking-parent/{id}")]
        Task<ApiErrorResult<DetailAppointmentBookingParentResult>> DetailAppointmentBookingParent(string id);

        [Put("/schedule/appointment-booking-parent")]
        Task<ApiErrorResult> ApsentAppointmentBookingParent([Body] ApsentAppointmentBookingParentRequest body);

        [Post("/schedule/appointment-booking-parent-teacher-venue-mapping")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetTeacherVenueMapping([Body]GetTeacherVenueMappingRequest Body);

        [Get("/schedule/appointment-booking-parent-child-detail")]
        Task<ApiErrorResult<IEnumerable<GetAllChildDateilByParentResult>>> GetAllChildDateilByParent(GetAllChildDateilByParentRequest query);

        [Get("/schedule/appointment-booking-parent-student")]
        Task<ApiErrorResult<IEnumerable<GetHomeroomStudent>>> GetStudentByAppointmentBooking(GetStudentByAppointmentBookingRequest query);

        [Get("/schedule/appointment-booking-by-user")]
        Task<ApiErrorResult<IEnumerable<GetAppointmentBookingByUserResult>>> GetAppointmentBookingByUser(GetAppointmentBookingByUserRequest query);

        [Post("/schedule/appointment-booking-date")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetAppointmentBookingDate([Body]GetAppointmentBookingDateRequest body);
        #endregion

        #region Available Time
        [Get("/schedule/appointment-booking/get-available-time")]
        Task<ApiErrorResult<GetAvailableTimeResult>> GetAvailableTime(GetAvailableTimeRequest query);
        #endregion

        #region Reschedule Invitation Booking
        [Put("/schedule/appointment-booking-reschedule-and-cancle")]
        Task<ApiErrorResult> UpdateInvitationBooking([Body] UpdateInvitationBookingRequest body);
        #endregion
    }
}
