using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendanceLongrun.Longrun.AttendanceSummaryTerm
{
    public class GetPesentStudentResult
    {
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string Class { get; set; }
        public TimeSpan? Tapping { get; set; }
        public List<GetPresentStudentSession> ListSession { get; set; }
    }
}
