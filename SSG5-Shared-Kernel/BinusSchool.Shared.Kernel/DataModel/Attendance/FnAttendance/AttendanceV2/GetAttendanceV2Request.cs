using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class GetAttendanceV2Request 
    {
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public int Semester { get; set; }
        public DateTime Date { get; set; }
        public string IdHomeroom { get; set; }
    }
}
