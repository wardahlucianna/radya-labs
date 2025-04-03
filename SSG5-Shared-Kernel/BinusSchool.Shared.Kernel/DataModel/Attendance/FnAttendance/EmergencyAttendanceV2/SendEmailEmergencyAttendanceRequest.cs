using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class SendEmailEmergencyAttendanceRequest
    {
        public string IdAttendanceQueue { get; set; }
        public List<string> studentEmergencyList {  get; set; }
    }
}
