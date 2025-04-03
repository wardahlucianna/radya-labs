using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceEntry;
using BinusSchool.Data.Model.Attendance.FnAttendance.EventAttendanceEntry;
using Refit;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IEventAttendanceEntry : IFnAttendance
    {
        [Get("/event-attendance-entry/information")]
        Task<ApiErrorResult<EventAttendanceInformationResult>> GetEventAttendanceInformation(GetEventAttendanceInformationRequest param);

        [Get("/event-attendance-entry/get-check")]
        Task<ApiErrorResult<IEnumerable<EventCheck>>> GetEventAttendanceCheck(GetEventAttendanceCheckRequest param);

        [Get("/event-attendance-entry/get-level")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetLevelsByEvent(GetLevelsByEventRequest param);

        [Get("/event-attendance-entry/get-grade")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetGradesByEvent(GetGradesByEventRequest param);

        [Get("/event-attendance-entry/get-subject")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetSubjectsByEvent(GetSubjectsByEventRequest param);

        [Get("/event-attendance-entry/get-homeroom")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetHomeroomsByEvent(GetHomeroomsByEventRequest param);

        [Get("/event-attendance-entry")]
        Task<ApiErrorResult<EventAttendanceEntryResult>> GetEventAttendanceEntry(GetEventAttendanceEntryRequest param);

        [Get("/event-attendance-entry/summary")]
        Task<ApiErrorResult<EventAttendanceSummaryResult>> GetEventAttendanceSummary(GetEventAttendanceSummaryRequest param);

        [Put("/event-attendance-entry")]
        Task<ApiErrorResult> UpdateEventAttendanceEntry([Body] UpdateEventAttendanceEntryRequest body);

        [Get("/event-attendance-entry/download-file")]
        Task<HttpResponseMessage> DownloadFileEventAttendanceEntry(DownloadFileAttendanceEntryRequest param);

        [Multipart]
        [Post("/event-attendance-entry/upload-file")]
        Task<ApiErrorResult<string>> UploadFileEventAttendanceEntry(StreamPart file, [Query] string IdEventCheck, [Query] string IdUserEvent);

        [Post("/event-attendance-entry/all-present")]
        Task<ApiErrorResult> AllPresentEventAttendanceEntry([Body] AllPresentEventAttendanceRequest body);

        [Post("/event-attendance-entry/all-present-excuse-absent")]
        Task<ApiErrorResult> AllPresentExcuseAbsent([Body] AllPresentAllExcuseEventAttendanceRequest body);
    }
}
