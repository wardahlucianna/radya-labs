using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.Attendance
{
    public class GetUnsubmittedAttendanceEventResult
    {
        public string idEvent { get; set; }
        public string eventName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string AttendanceCheckName { get; set; }
        public TimeSpan AttendanceTime { get; set; }
    }
}
