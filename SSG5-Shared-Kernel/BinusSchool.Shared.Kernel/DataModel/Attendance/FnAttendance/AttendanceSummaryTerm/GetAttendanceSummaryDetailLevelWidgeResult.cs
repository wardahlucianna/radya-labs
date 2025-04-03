using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailLevelWidgeResult
    {
        public int Pending { get; set;}
        public int Unsubmited { get; set;}
        public bool IsShowPending { get; set;}
    }
}
