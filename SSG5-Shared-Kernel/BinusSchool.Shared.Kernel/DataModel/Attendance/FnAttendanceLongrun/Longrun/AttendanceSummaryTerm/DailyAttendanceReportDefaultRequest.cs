using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BinusSchool.Data.Model.Attendance.FnAttendanceLongrun.Longrun.AttendanceSummaryTerm
{
    public class DailyAttendanceReportDefaultRequest
    {
        public DailyAttendanceReportRequest Body { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public DateTime Date { get; set; }
    }

}
