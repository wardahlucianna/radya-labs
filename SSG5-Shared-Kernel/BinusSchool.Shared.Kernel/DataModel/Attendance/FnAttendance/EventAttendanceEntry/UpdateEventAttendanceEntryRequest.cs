using System.Collections.Generic;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.EventAttendanceEntry
{
    public class UpdateEventAttendanceEntryRequest
    {
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
