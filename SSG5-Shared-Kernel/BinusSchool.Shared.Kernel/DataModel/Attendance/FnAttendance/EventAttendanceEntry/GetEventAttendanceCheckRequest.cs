using System;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.EventAttendanceEntry
{
    public class GetEventAttendanceCheckRequest
    {
        public string IdEvent { get; set; }
        public DateTime Date { get; set; }
    }
}
