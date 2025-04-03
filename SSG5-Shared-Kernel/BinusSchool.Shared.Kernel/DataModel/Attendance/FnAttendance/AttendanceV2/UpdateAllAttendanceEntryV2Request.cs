using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class UpdateAllAttendanceEntryV2Request
    {
        public string IdScheduleLesson { get; set; }
        public string IdHomeroom { get; set; }
        public string ClassId { get; set; }
        public DateTime Date { get; set; }
        public string IdSession { get; set; }
    }
}
