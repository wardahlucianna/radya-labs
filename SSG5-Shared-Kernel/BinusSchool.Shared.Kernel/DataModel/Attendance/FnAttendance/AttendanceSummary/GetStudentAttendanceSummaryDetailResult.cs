using System;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetStudentAttendanceSummaryDayDetailResult
    {
        public DateTime Date { get; set; }
        public string TeacherName { get; set; }
        public string AttendanceName { get; set; }
        public string Reason { get; set; }
    }
    public class GetStudentAttendanceSummarySessionDetailResult : GetStudentAttendanceSummaryDayDetailResult
    {
        public string Session { get; set; }
        public string SubjectName { get; set; }
    }
}
