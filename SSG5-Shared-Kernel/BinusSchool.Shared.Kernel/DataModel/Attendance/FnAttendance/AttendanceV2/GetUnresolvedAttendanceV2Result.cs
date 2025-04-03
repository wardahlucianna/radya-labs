using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class GetUnresolvedAttendanceV2Result
    {
        public bool IsShowingPopup { get; set; }
        public List<UnresolvedAttendanceGroupV2Result> Attendances { get; set; }
        public int TotalUnsubmitted { get; set; }
    }
}
