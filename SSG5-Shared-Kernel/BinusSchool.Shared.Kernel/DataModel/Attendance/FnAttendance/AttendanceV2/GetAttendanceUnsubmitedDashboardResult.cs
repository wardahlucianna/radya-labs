using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class GetAttendanceUnsubmitedDashboardResult
    {
        public bool IsShowingPopup { get; set; }
        public List<UnresolvedAttendanceGroupV2Result> Attendances { get; set; }
        public int countUnsubmited { get; set; }
    }
}
