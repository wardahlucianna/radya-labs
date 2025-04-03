using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceEntry;
using Refit;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IAttendanceEntry : IFnAttendance
    {
        [Get("/attendance-entry")]
        Task<ApiErrorResult<GetAttendanceEntryResult>> GetAttendanceEntry(GetAttendanceEntryRequest param);

        [Put("/attendance-entry")]
        Task<ApiErrorResult> UpdateAttendanceEntry([Body] UpdateAttendanceEntryRequest body);

        [Put("/attendance-entry/all-present")]
        Task<ApiErrorResult> UpdateAllAttendanceEntry([Body] UpdateAllAttendanceEntryRequest body);

        [Get("/attendance-entry/download-file")]
        Task<HttpResponseMessage> DownloadFileAttendanceEntry(DownloadFileAttendanceEntryRequest param);

        [Multipart]
        [Post("/attendance-entry/upload-file")]
        Task<ApiErrorResult<string>> UploadFileAttendanceEntry(StreamPart file, [Query] string idGeneratedScheduleLesson);
    }
}
