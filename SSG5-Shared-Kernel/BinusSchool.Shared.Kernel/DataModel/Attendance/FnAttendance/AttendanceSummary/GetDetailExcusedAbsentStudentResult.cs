using System;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetDetailExcusedAbsentStudentResult
    {
        public DateTime Date { get; set; }
        public string Attendance { get; set; }
        public string SessionNo { get; set; }
        public string SubjectName { get; set; }
        public string IdHomeroom { get; set; }
        public string TeacherName { get; set; }
        public string Reason { get; set; }
        public string FileEvidence { get; set; }
    }
}
