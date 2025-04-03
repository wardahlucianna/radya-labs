using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class UpdateEmergencyAttendanceReportRequest
    {
        public string IdEmergencyReport { get; set; }
        public string IdUserAction { get; set; }
        public bool SendEmailStatus { get; set; }
    }
}
