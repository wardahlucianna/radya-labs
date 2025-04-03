using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.TeacherHomeroomAndSubjectTeacher;
using Refit;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IAttendance : IFnAttendance
    {
        [Get("/attendance/homeroom")]
        Task<ApiErrorResult<IEnumerable<HomeroomAttendanceResult>>> GetHomeroomAttendance(GetAttendanceRequest query);

        [Get("/attendance/session")]
        Task<ApiErrorResult<IEnumerable<SessionAttendanceResult>>> GetSessionAttendance(GetAttendanceRequest query);

        [Get("/attendance/event")]
        Task<ApiErrorResult<IEnumerable<GetEventAttendanceResult>>> GetEventAttendance(GetEventAttendanceRequest query);

        [Get("/attendance/pending")]
        Task<ApiErrorResult<GetUnresolvedAttendanceResult>> GetPendingAttendance(GetUnresolvedAttendanceRequest query);

        [Get("/attendance/pending-term-day")]
        Task<ApiErrorResult<GetUnresolvedAttendanceResult>> GetPendingAttendanceTermDay(GetUnresolvedAttendanceRequest query);

        [Get("/attendance/unsubmitted")]
        Task<ApiErrorResult<GetUnresolvedAttendanceResult>> GetUnsubmittedAttendance(GetUnresolvedAttendanceRequest query);        
        
        [Get("/attendance/unsubmitted-term-day")]
        Task<ApiErrorResult<GetUnresolvedAttendanceResult>> GetUnsubmittedAttendanceTermDay(GetUnresolvedAttendanceRequest query);

        [Get("/attendance/conflicted-event")]
        Task<ApiErrorResult<IEnumerable<GetStudentConflictedEventsResult>>> GetConflictedEvent();

        [Post("/attendance/conflicted-event")]
        Task<ApiErrorResult> ChooseConflictedEvent([Body] ChooseConflictedEventRequest body);

        [Get("/attendance/teacher-assignment")]
        Task<ApiErrorResult<TeacherHomeroomAndSubjectTeacherResult>> GetTeacherAssignment(TeacherHomeroomAndSubjectTeacherRequest query);

        [Get("/attendance/unsubmitted-event")]
        Task<ApiErrorResult<IEnumerable<GetUnsubmittedAttendanceEventResult>>> GetUnsubmittedAttendanceEvent(GetUnsubmittedAttendanceEventRequest query);

        #region Master Data Attendance
        [Get("/attendance/master-data-attendance")]
        Task<ApiErrorResult<IEnumerable<GetMasterDataAttendanceResult>>> GetMasterDataAttendances(GetMasterDataAttendanceRequest request);

        [Get("/attendance/master-data-attendance/{id}")]
        Task<ApiErrorResult<MasterDataAttendanceDetailResult>> GetMasterDataAttendanceDetail(string id);

        [Post("/attendance/master-data-attendance")]
        Task<ApiErrorResult> AddMasterDataAttendance([Body] AddMasterDataAttendanceRequest body);

        [Put("/attendance/master-data-attendance")]
        Task<ApiErrorResult> UpdateMasterDataAttendance([Body] UpdateMasterDataAttendanceRequest body);

        [Delete("/attendance/master-data-attendance")]
        Task<ApiErrorResult> DeleteMasterDataAttendance([Body] IEnumerable<string> ids);
        #endregion
    }
}
