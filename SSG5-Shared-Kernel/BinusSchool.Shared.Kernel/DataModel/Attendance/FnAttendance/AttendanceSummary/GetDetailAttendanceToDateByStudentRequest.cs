using System;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetDetailAttendanceToDateByStudentRequest
    {
        public string IdStudent { get; set; }
        public string IdLevel { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Semester { get; set; }
    }
}
