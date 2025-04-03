using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class GetUnresolvedAttendanceV2Request
    {
        public string IdUser { get; set; }
        public string IdAcademicYear { get; set; }
        public string CurrentPosition { get; set; }
    }
}
