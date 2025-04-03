using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Data.Models.Binusian.BinusSchool.AttendanceLog;
using Refit;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IAttendanceV2 : IFnAttendance
    {
        #region List Attendance Homeroom Teacher
        [Get("/attendanceV2")]
        Task<ApiErrorResult<IEnumerable<HomeroomAttendanceV2Result>>> GetHomeroomAttendanceV2(GetAttendanceV2Request query);

        [Get("/attendanceV2/unsubmitted")]
        Task<ApiErrorResult<GetUnresolvedAttendanceV2Result>> GetUnsubmittedAttendanceV2(GetUnresolvedAttendanceV2Request query);

        [Get("/attendanceV2/pending")]
        Task<ApiErrorResult<GetUnresolvedAttendanceV2Result>> GetPendingAttendanceV2(GetUnresolvedAttendanceV2Request query);

        [Get("/attendanceV2/unsubmitted-event")]
        Task<ApiErrorResult<IEnumerable<GetUnsubmittedAttendanceEventV2Result>>> GetUnsubmittedAttendanceEventV2(GetUnsubmittedAttendanceEventV2Request query);

        [Get("/attendanceV2/class-id")]
        Task<ApiErrorResult<IEnumerable<GetClassIdByHomeroomResult>>> GetClassIdByHomeroom(GetAttendanceV2Request query);
        #endregion

        #region Detail Attendance Homeroom Teacher
        [Get("/attendanceV2-entry")]
        Task<ApiErrorResult<GetAttendanceEntryV2Result>> GetAttendanceDetailV2(GetAttendanceEntryV2Request query);

        [Put("/attendanceV2-entry")]
        Task<ApiErrorResult> UpdateAttendanceEntryV2Handler([Body] UpdateAttendanceEntryV2Request body);
        #endregion

        #region List Attendance Subject Teacher
        [Get("/attendanceV2/session")]
        Task<ApiErrorResult<IEnumerable<SessionAttendanceV2Result>>> GetSessionAttendanceV2(GetAttendanceV2Request query);

        [Put("/attendanceV2-entry/all-present")]
        Task<ApiErrorResult> UpdateAllAttendanceDetailV2([Body] UpdateAllAttendanceEntryV2Request body);
        #endregion

        #region Attendance Event
        [Post("/attendanceV2-event/all-present")]
        Task<ApiErrorResult> AllPresentEventAttendanceV2([Body] AllPresentEventAttendanceV2Request body);

        [Put("/attendanceV2-event")]
        Task<ApiErrorResult> UpdateEventAttendanceDetailV2([Body] UpdateEventAttendanceEntryV2Request body);
        #endregion

        #region dashboard
        [Post("/attendanceV2/unsubmited-dashboard")]
        Task<ApiErrorResult<GetUnresolvedAttendanceV2Result>> GetAttendanceUnsubmitedDashboard([Body]GetAttendanceUnsubmitedDashboardRequest body);
        #endregion

        #region Update attendance from Moving
        [Post("/attendanceV2/move-student")]
        Task<ApiErrorResult> UpdateAttendanceByMoveStudentEnroll([Body] UpdateAttendanceByMoveStudentEnrollRequest body);
        #endregion

        [Post("/attendanceV2-machine")]
        Task<ApiErrorResult<IEnumerable<AttendanceLog>>> GetAttendanceLogs([Body] GetAttendanceLogRequest body);

        

    }
}
