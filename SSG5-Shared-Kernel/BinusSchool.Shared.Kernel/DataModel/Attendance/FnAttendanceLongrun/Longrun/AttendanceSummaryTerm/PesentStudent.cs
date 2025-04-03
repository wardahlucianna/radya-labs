using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendanceLongrun.Longrun.AttendanceSummaryTerm
{
    public class PesentStudent : GetPresentStudentSession
    {
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string Class { get; set; }
    }
}
