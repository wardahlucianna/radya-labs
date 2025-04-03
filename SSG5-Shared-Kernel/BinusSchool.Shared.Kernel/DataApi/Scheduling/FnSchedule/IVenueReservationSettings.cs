using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using System.Threading.Tasks;
using Refit;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservationSettings.MasterSpecialRoleVenueReservation;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservationSettings.MasterDayRestriction;


namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IVenueReservationSettings : IFnSchedule
    {
        #region Master Special Role Venue Reservation
        [Get("/schedule/venue-reservation-settings/get-list-special-role-venue")]
        Task<ApiErrorResult<IEnumerable<GetListSpecialRoleVenueResult>>> GetlistSpecialRoleVenue(GetListSpecialRoleVenueRequest query);

        [Get("/schedule/venue-reservation-settings/get-detail-special-role-venue")]
        Task<ApiErrorResult<GetDetailSpecialRoleVenueResult>> GetDetailSpecialRoleVenue(GetDetailSpecialRoleVenueRequest query);

        [Post("/schedule/venue-reservation-settings/delete-special-role-venue")]
        Task<ApiErrorResult> DeleteSpecialRoleVenue([Body] DeleteSpecialRoleVenueRequest body);

        [Post("/schedule/venue-reservation-settings/save-special-role-venue")]
        Task<ApiErrorResult> SaveSpecialRoleVenue([Body] SaveSpecialRoleVenueRequest body);
        #endregion

        #region Master Day Restriction
        [Get("/schedule/venue-reservation-settings/get-list-master-day-restriction")]
        Task<ApiErrorResult<IEnumerable<GetListMasterDayRestrictionResult>>> GetListMasterDayRestriction(GetListMasterDayRestrictionRequest query);

        [Get("/schedule/venue-reservation-settings/get-detail-master-day-restriction")]
        Task<ApiErrorResult<GetDetailMasterDayRestrictionResult>> GetDetailMasterDayRestriction(GetDetailMasterDayRestrictionRequest query);

        [Post("/schedule/venue-reservation-settings/delete-master-day-restriction")]
        Task<ApiErrorResult> DeleteMasterDayRestriction([Body] DeleteMasterDayRestrictionRequest body);

        [Post("/schedule/venue-reservation-settings/save-master-day-restriction")]
        Task<ApiErrorResult> SaveMasterDayRestriction([Body] SaveMasterDayRestrictionRequest body);
        #endregion
    }
}
