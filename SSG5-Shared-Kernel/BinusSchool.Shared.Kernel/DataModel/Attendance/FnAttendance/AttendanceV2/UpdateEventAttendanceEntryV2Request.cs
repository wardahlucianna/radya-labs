using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class UpdateEventAttendanceEntryV2Request
    {
        public string IdUser { get; set; }
        public string IdEventCheck { get; set; }
        public string IdLevel { get; set; }
        public IEnumerable<UpdateEventAttendanceEntryStudent> Entries { get; set; }
    }

    public class UpdateEventAttendanceEntryStudent
    {
        public string IdUserEvent { get; set; }
        public string IdAttendanceMapAttendance { get; set; }
        public IEnumerable<string> IdWorkhabits { get; set; }
        public string LateInMinute { get; set; }
        public string File { get; set; }
        public string Note { get; set; }
    }
}
