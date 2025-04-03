using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailSchoolDayWidgeResult
    {
        public int Pending { get; set; }
        public int Unsubmited { get; set; }
        public int Present { get; set; }
        public int Late { get; set; }
        public int ExcusedAbsence { get; set; }
        public int UnexcusedAbsence { get; set; }
        public bool IsShowPending { get; set; }
    }
}
