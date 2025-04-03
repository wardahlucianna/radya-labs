using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class ATD7V2NotificationModels
    {
        public string IdEvent { get; set; }
        public string EventName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string AttendanceCheckName { get; set; }
        public TimeSpan AttendanceTime { get; set; }
        public string IdUser { get; set; }
        public string Link { get; set; }
    }
}
