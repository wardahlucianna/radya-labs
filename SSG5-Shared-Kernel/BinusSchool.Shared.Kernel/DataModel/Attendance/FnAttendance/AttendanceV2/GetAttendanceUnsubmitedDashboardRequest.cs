using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class GetAttendanceUnsubmitedDashboardRequest 
    {
        public string IdAcademicYear { get; set; }
        public List<string> SelectedPosition { get; set; }
        public string IdUser { get; set; }
    }
}
