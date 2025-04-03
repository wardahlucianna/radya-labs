using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryWidgeResult
    {
        public int Submited { get; set; }
        public int Unsubmitted { get; set; }
        public int Pending { get; set; }
        public int TotalStudent { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
