using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryReport
{
    public class ExportExcelAttandanceSummaryUAPresentDailyRequest
    {
        public DateTime AttendanceDate { get; set; }
        public GetAttendanceSummaryDailyReportResult DataAttandance { get; set; }
    }
}
