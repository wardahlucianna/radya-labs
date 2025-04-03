using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class GetAttendanceEntryV2Request
    {
        public DateTime Date { get; set; }
        public string ClassId { get; set; }
        public string IdSession { get; set; }
        public AttendanceEntryStatus? Status { get; set; }
        public string IdUser { get; set; }
        public string CurrentPosition { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdHomeroom { get; set; }
    }
}
