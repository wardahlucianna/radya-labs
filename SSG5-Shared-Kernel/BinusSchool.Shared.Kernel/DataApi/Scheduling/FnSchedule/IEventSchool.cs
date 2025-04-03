using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolsEvent;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IEventSchool : IFnSchedule
    {
        [Get("/schedule/school-event")]
        Task<ApiErrorResult<IEnumerable<GetSchoolEventResult>>> GetSchoolEvent(GetSchoolEventRequest query);

        [Get("/schedule/school-event/academic-calendar")]
        Task<ApiErrorResult<IEnumerable<GetSchoolEventAcademicResult>>> GetSchoolEventOfAcademicCalendar(GetSchoolEventAcademicRequest query);

        [Post("/schedule/school-event")]
        Task<ApiErrorResult> AddSchoolEvent([Body] AddSchoolEventRequest body);

        [Put("/schedule/school-event")]
        Task<ApiErrorResult> UpdateSchoolEvent([Body] UpdateSchoolEventRequest body);

        [Delete("/schedule/school-event")]
        Task<ApiErrorResult> DeleteSchoolEvent([Body] IEnumerable<string> ids);

        [Get("/schedule/school-event/get-user-by-role-position")]
        Task<ApiErrorResult<IEnumerable<GetUserByRolePositionResult>>> GetUserByRolePosition(GetUserByRolePositionRequest query);

        [Get("/schedule/school-event/default-event-approver-setting")]
        Task<ApiErrorResult<GetDefaultEventApproverSettingResult>> GetDefaultEventApproverSetting(GetDefaultEventApproverSettingRequest query);

        [Put("/schedule/school-event/set-default-event-approver-setting")]
        Task<ApiErrorResult> SetDefaultEventApproverSetting([Body] SetDefaultEventApproverSettingRequest body);

        [Get("/schedule/school-event-approval")]
        Task<ApiErrorResult<IEnumerable<GetSchoolEventApprovalResult>>> GetSchoolEventApproval(GetSchoolEventApprovalRequest query);

        [Get("/schedule/get-student-participant-from-event-setting")]
        Task<ApiErrorResult<IEnumerable<GetStudentParticipantResult>>> GetStudentParticipant(GetStudentParticipantRequest query);
    
        [Put("/schedule/school-event/set-school-event-approval-status")]
        Task<ApiErrorResult> SetSchoolEventApprovalStatus([Body] SetSchoolEventApprovalStatusRequest body);

        [Get("/schedule/school-event-calender")]
        Task<ApiErrorResult<IEnumerable<GetEventCalendarResult>>> GetSchoolEventCalender(GetEventCalendarRequest query);

        [Get("/schedule/school-event-involvement")]
        Task<ApiErrorResult<IEnumerable<GetSchoolEventInvolvementResult>>> GetSchoolEventInvolvement(GetSchoolEventInvolvementRequest query);

        [Post("/schedule/school-event-involvement")]
        Task<ApiErrorResult> CreateSchoolEventInvolvement([Body] CreateSchoolEventInvolvementRequest body);

        [Put("/schedule/school-event-involvement-update")]
        Task<ApiErrorResult> UpdateSchoolEventInvolvement([Body] UpdateSchoolEventInvolvementRequest body);

        [Delete("/schedule/school-event-involvement-delete")]
        Task<ApiErrorResult> DeleteSchoolEventInvolvement([Body] DeleteSchoolEventInvolvementRequest body);

        [Get("/schedule/school-event-involvement-teacher")]
        Task<ApiErrorResult<IEnumerable<GetSchoolEventInvolvementTeacherResult>>> GetSchoolEventInvolvementTeacher(GetSchoolEventInvolvementTeacherRequest query);

        [Get("/schedule/get-list-parent-student")]
        Task<ApiErrorResult<IEnumerable<GetListParentStudentResult>>> GetListParentStudent(GetListParentStudentRequest query);

        [Get("/schedule/get-list-activity")]
        Task<ApiErrorResult<IEnumerable<GetDropdownActivityResult>>> GetListActivity(GetDropdownActivityRequest query);

        [Get("/schedule/get-list-award")]
        Task<ApiErrorResult<IEnumerable<GetDropdownAwardResult>>> GetListAward(GetDropdownAwardRequest query);

        [Get("/schedule/school-event/student-by-homeroom")]
        Task<ApiErrorResult<IEnumerable<GetStudentbyHomeromeResult>>> GetStudentbyHomeromeStudent(GetStudentbyHomeromeRequest query);

        [Get("/schedule/school-event/student-by-grade-semester")]
        Task<ApiErrorResult<IEnumerable<GetStudentbyHomeromeResult>>> GetStudentbyGradeSemester(GetStudentbyGradeSemesterRequest query);

        [Get("/schedule/school-event-summary")]
        Task<ApiErrorResult<IEnumerable<GetSchoolEventSummary2Result>>> GetSchoolEventSummary(GetSchoolEventSummaryRequest query);

        [Get("/schedule/school-event-setting-detail")]
        Task<ApiErrorResult<DetailEventSettingResult>> DetailEventSetting(DetailEventSettingRequest query);

        [Get("/schedule/school-event-summary/download-excel")]
        Task<HttpResponseMessage> DownloadExcelSchedule(DownloadEventSummaryRequest query);

        [Get("/schedule/detail-record-of-involvement")]
        Task<ApiErrorResult<DetailRecordOfInvolvementResult>> DetailRecordOfInvolvement(DetailRecordOfInvolvementRequest query);

        [Get("/schedule/detail-record-of-involvement-teacher")]
        Task<ApiErrorResult<DetailRecordOfInvolvementTeacherResult>> DetailRecordOfInvolvementTeacher(DetailRecordOfInvolvementTeacherRequest query);
        
        [Get("/schedule/school-event-history")]
        Task<ApiErrorResult<IEnumerable<GetSchoolEventHistoryResult>>> GetSchoolEventHistory(GetSchoolEventHistoryRequest query);

        [Get("/schedule/get-data-activity-for-merit")]
        Task<ApiErrorResult<IEnumerable<GetDataActivityForMeritResult>>> GetDataActivityForMerit(GetDataActivityForMeritRequest query);

        [Post("/schedule/create-activity-to-merit")]
        Task<ApiErrorResult> CreateActivityDataToMerit([Body] CreateActivityDataToMeritRequest body);

        [Get("/schedule/get-download-student-certification")]
        Task<ApiErrorResult<GetDownloadStudentCertificationResult>> GetDownloadStudentCertification(GetDownloadStudentCertificationRequest query);

        [Get("/schedule/get-download-student-certificate")]
        Task<ApiErrorResult<GetDownloadStudentCertificateResult>> GetDownloadStudentCertificate(GetDownloadStudentCertificateRequest query);

        [Get("/schedule/detail-history-event")]
        Task<ApiErrorResult<DetailHistoryEventResult>> DetailHistoryEvent(DetailEventSettingRequest query);

        [Post("/schedule/add-history-event")]
        Task<ApiErrorResult> AddHistoryEvent([Body] AddHistoryEventRequest body);

        [Get("/schedule/get-list-parent-permission")]
        Task<ApiErrorResult<IEnumerable<GetListParentPermissionResult>>> GetListParentPermission(GetListParentPermissionRequest query);

        #region Event Schedule
        [Post("/schedule/update-status-event-school")]
        Task<ApiErrorResult> UpdateStatusSyncEventSchedule([Body] UpdateStatusSyncEventScheduleRequest body);

        [Get("/schedule/event-schedule")]
        Task<ApiErrorResult<IEnumerable<GetEventScheduleResult>>> GetEventSchedule(GetEventScheduleRequest param);
        #endregion

        [Get("/schedule/school-event/queue")]
        Task<ApiErrorResult> QueueEvent(QueueEventRequest query);
    }
}
