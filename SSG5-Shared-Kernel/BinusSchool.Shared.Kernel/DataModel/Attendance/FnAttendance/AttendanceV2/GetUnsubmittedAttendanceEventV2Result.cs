using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class GetUnsubmittedAttendanceEventV2Result
    {
        public string IdEvent { get; set; }
        public string EventName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string AttendanceCheckName { get; set; }
        public TimeSpan AttendanceTime { get; set; }
    }
}
