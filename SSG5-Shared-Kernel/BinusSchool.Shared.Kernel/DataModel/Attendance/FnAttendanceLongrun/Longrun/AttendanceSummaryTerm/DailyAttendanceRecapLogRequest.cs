using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BinusSchool.Data.Model.Attendance.FnAttendanceLongrun.Longrun.AttendanceSummaryTerm
{
    public class DailyAttendanceRecapLogRequest
    {
        public DailyAttendanceReportRequest Body { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public bool IsDone { get; set; }
        public bool IsError { get; set; }
        public string Message { get; set; }
    }
}
