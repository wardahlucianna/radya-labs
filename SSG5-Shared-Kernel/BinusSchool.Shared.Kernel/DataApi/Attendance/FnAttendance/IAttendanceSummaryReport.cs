using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryReport;
using Refit;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using System.Net.Http;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IAttendanceSummaryReport : IFnAttendance
    {
        [Get("/attendance-summary-report/GetAttendanceSummaryDailyReport")]
        Task<ApiErrorResult<GetAttendanceSummaryDailyReportResult>> GetAttendanceSummaryDailyReport(GetAttendanceSummaryDailyReportRequest query);

        [Post("/attendance-summary-report/ExportExcelAttandanceSummaryUAPresentDaily")]
        Task<HttpResponseMessage> ExportExcelAttandanceSummaryUAPresentDaily(ExportExcelAttandanceSummaryUAPresentDailyRequest query);
    }
}
