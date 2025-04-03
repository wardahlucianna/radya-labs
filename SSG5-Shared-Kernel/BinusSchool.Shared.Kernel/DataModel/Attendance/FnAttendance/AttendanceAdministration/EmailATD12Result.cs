using System;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration
{
    public class EmailATD12Result
    {
        public string IdStudent { get; set; }
        public string IdTeacher { get; set; }
        public string TeacherName { get; set; }
        public string ClassID { get; set; }
        public string SessionID { get; set; }
        public DateTime Date { get; set; }
        public string Reason { get; set; }
    }
}