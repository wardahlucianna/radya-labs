using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using Refit;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceRecap;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using System.Net.Http;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IAttendanceRecap : IFnAttendance
    {
        [Get("/attendance-recap")]
        Task<ApiErrorResult<IEnumerable<GetAttendanceRecapResult>>> GetAttendanaceRecap(GetAttendanceRecapRequest query);
        [Get("/attendance-recap/detail")]
        Task<ApiErrorResult<GetAttendanceRecapResult>> GetDetailAttendanceRecap(GetDetailAttendanceRecapRequest query);
        [Get("/attendance-recap/unsubmitted")]
        Task<ApiErrorResult<IEnumerable<GetAttendanceSummaryUnsubmitedResult>>> GetUnsubmittedAttendanceRecap(GetDetailAttendanceRecapRequest query);
        [Get("/attendance-recap/pending")]
        Task<ApiErrorResult<IEnumerable<GetDataDetailAttendanceRecapResult>>> GetPendingAttendanceRecap(GetDetailAttendanceRecapRequest query);
        [Get("/attendance-recap/present")]
        Task<ApiErrorResult<IEnumerable<GetDataDetailAttendanceRecapResult>>> GetPresentAttendanceRecap(GetDetailAttendanceRecapRequest query);
        [Get("/attendance-recap/late")]
        Task<ApiErrorResult<IEnumerable<GetDataDetailAttendanceRecapResult>>> GetLateAttendanceRecap(GetDetailAttendanceRecapRequest query);
        [Get("/attendance-recap/unexcused")]
        Task<ApiErrorResult<IEnumerable<GetDataDetailAttendanceRecapResult>>> GetUnexcusedAttendanceRecap(GetDetailAttendanceRecapRequest query);
        [Get("/attendance-recap/excused")]
        Task<ApiErrorResult<IEnumerable<GetDataDetailAttendanceRecapResult>>> GetExcusedAttendanceRecap(GetDetailAttendanceRecapRequest query);
        [Get("/attendance-recap/download")]
        Task<HttpResponseMessage> GetDownloadAttendanceRecap(GetDetailAttendanceRecapRequest query);
        [Get("/attendance-recap/lates-update")]
        Task<ApiErrorResult<DateTime>> GetLatesUpdateAttendanceRecap();
    }
}
