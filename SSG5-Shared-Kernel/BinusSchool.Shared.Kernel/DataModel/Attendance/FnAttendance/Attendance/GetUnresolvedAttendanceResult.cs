using System.Collections.Generic;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.Attendance
{
    public class GetUnresolvedAttendanceResult
    {
        public bool IsShowingPopup { get; set; }
        public List<UnresolvedAttendanceGroupResult> Attendances { get; set; }
        public List<UnresolvedEventAttendanceResult> EventAttendance { get; set; }
    }
}
