using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.BookingPeriodSetting;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerAllocation;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerReservation;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.StudentBooking;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface ILockerReservation : IFnStudent
    {
        #region Locker Booking Period Setting
        [Get("/locker-reservation/get-locker-booking-period-setting")]
        Task<ApiErrorResult<IEnumerable<GetLockerBookingPeriodSettingResult>>> GetLockerBookingPeriodSetting(GetLockerBookingPeriodSettingRequest query);

        [Get("/locker-reservation/get-locker-booking-period-grade")]
        Task<ApiErrorResult<IEnumerable<GetLockerBookingPeriodGradeResult>>> GetLockerBookingPeriodGrade(GetLockerBookingPeriodGradeRequest query);

        [Post("/locker-reservation/save-locker-booking-period-grade")]
        Task<ApiErrorResult> SaveLockerBookingPeriodGrade([Body] SaveLockerBookingPeriodGradeRequest param);

        [Get("/locker-reservation/get-locker-reservation-period-policy")]
        Task<ApiErrorResult<IEnumerable<GetLockerReservationPeriodPolicyResult>>> GetLockerReservationPeriodPolicy(GetLockerReservationPeriodPolicyRequest query);

        [Put("/locker-reservation/update-locker-reservation-period-policy")]
        Task<ApiErrorResult> UpdateLockerReservationPeriodPolicy([Body] UpdateLockerReservationPeriodPolicyRequest[] param);

        [Delete("/locker-reservation/delete-locker-reservation-period")]
        Task<ApiErrorResult> DeleteLockerReservationPeriod([Body] DeleteLockerReservationPeriodRequest param);
        #endregion

        #region Locker Allocation
        [Get("/locker-reservation/get-all-locker-allocation")]
        Task<ApiErrorResult<IEnumerable<GetAllLockerAllocationResult>>> GetAllLockerAllocation(GetAllLockerAllocationRequest query);

        [Get("/locker-reservation/get-locker-allocation-building")]
        Task<ApiErrorResult<IEnumerable<GetLockerAllocationBuildingResult>>> GetLockerAllocationBuilding(GetLockerAllocationBuildingRequest query);

        [Get("/locker-reservation/get-locker-allocation-floor")]
        Task<ApiErrorResult<IEnumerable<GetLockerAllocationFloorResult>>> GetLockerAllocationFloor(GetLockerAllocationFloorRequest query);

        [Get("/locker-reservation/get-locker-allocation-grade")]
        Task<ApiErrorResult<IEnumerable<GetLockerAllocationGradeResult>>> GetLockerAllocationGrade(GetLockerAllocationGradeRequest query);

        [Post("/locker-reservation/save-locker-allocation")]
        Task<ApiErrorResult> SaveLockerAllocation([Body] SaveLockerAllocationRequest param);

        [Delete("/locker-reservation/delete-locker-allocation")]
        Task<ApiErrorResult> DeleteLockerAllocation([Body] DeleteLockerAllocationRequest param);

        [Post("/locker-reservation/copy-locker-allocation-from-last-semester")]
        Task<ApiErrorResult> CopyLockerAllocationFromLastSemester([Body] CopyLockerAllocationFromLastSemesterRequest param);
        #endregion

        #region Locker Reservation
        [Post("/locker-reservation/get-list-locker-reservation")]
        Task<ApiErrorResult<IEnumerable<GetListLockerReservationResult>>> GetListLockerReservation([Body] GetListLockerReservationRequest param);

        [Post("/locker-reservation/copy-locker-reservation")]
        Task<ApiErrorResult> CopyLockerReservation([Body] CopyLockerReservationRequest param);

        [Post("/locker-reservation/get-locker-list")]
        Task<ApiErrorResult<GetLockerListResult>> GetLockerList([Body] GetLockerListRequest param);

        [Delete("/locker-reservation/delete-locker-reservation")]
        Task<ApiErrorResult> DeleteLockerReservation([Body] DeleteLockerReservationRequest param);

        [Post("/locker-reservation/save-locked-locker")]
        Task<ApiErrorResult> SaveLockedLocker([Body] SaveLockedLockerRequest param);

        [Post("/locker-reservation/update-locked-locker")]
        Task<ApiErrorResult> UpdateLockedLocker([Body] UpdateLockedLockerRequest param);

        [Post("/locker-reservation/save-locker-reservation")]
        Task<ApiErrorResult> SaveLockerReservation([Body] SaveLockerReservationRequest param);

        [Post("/locker-reservation/export-excel-summary-locker-reservation")]
        Task<HttpResponseMessage> ExportExcelSummaryLockerReservation([Body] ExportExcelSummaryLockerReservationRequest body);
        #endregion

        #region Student Booking 
        [Get("/locker-reservation/get-student-booking-locker")]
        Task<ApiErrorResult<GetStudentBookingResult>> GetStudentBooking(GetStudentBookingRequest query);

        [Post("/locker-reservation/add-student-reservation-locker")]
        Task<ApiErrorResult> AddStudentReservation([Body] AddStudentReservationRequest param);
        #endregion
    }
}
