using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class SendEmailEmergencyAttendanceResult
    {
        public bool status {  get; set; }
        public string msg { get; set; }
        public int successCount { get; set; }
        public int failedCount { get; set; }
    }
}
