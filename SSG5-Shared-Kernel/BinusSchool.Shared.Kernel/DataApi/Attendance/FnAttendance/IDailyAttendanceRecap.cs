using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.DailyAttendanceRecap;
using Refit;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IDailyAttendanceRecap : IFnAttendance
    {
        [Get("/daily-attendance-recap")]
        Task<ApiErrorResult<IEnumerable<GetDailyAttendanceRecapResult>>> GetDailyAttendanceRecap(GetDailyAttendanceRecapRequest param);

        [Post("/daily-attendance-recap/download")]
        Task<HttpResponseMessage> GenerateExcelDailyAttendanceRecap([Body] GenerateExcelDailyAttendanceRecapRequest body);
    }
}
