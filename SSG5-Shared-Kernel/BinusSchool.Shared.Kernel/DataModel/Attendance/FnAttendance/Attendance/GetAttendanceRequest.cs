using System;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.Attendance
{
    public class GetAttendanceRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public int Semester { get; set; }
        public DateTime Date { get; set; }
        public string IdHomeroom { get; set; }
        public string IdSchool { get; set; }
    }
}
