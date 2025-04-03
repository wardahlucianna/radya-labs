using System;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetStudentWorkhabitDayDetailResult
    {
        public DateTime Date { get; set; }
        public string TeacherName { get; set; }
        public string Comment { get; set; }
    }
    public class GetStudentWorkhabitSessionDetailResult : GetStudentWorkhabitDayDetailResult
    {
        public string Session { get; set; }
        public string SubjectName { get; set; }
    }
}
