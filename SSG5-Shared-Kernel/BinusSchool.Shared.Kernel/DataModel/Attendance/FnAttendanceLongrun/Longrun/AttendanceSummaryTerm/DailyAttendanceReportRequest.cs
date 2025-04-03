using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendanceLongrun.Longrun.AttendanceSummaryTerm
{
    public class DailyAttendanceReportRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public string SelectedPosition { get; set; }
        public string IdSchool { get; set; }
    }
}
