using System;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceEntry
{
    public class GetAttendanceEntryRequest
    {
        public string IdHomeroom { get; set; }
        public DateTime Date { get; set; }
        public string ClassId { get; set; }
        public string IdSession { get; set; }
        public AttendanceEntryStatus? Status { get; set; }
        public string IdUser { get; set; }
        public string CurrentPosition { get; set; }
        public string IdSchool { get; set; }
    }
}
