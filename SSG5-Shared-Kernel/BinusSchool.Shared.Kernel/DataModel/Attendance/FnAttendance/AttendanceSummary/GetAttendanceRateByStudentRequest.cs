using System;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetAttendanceRateByStudentRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public string SelectedPosition { get; set; }
        public string IdStudent { get; set; }
        //public string IdHomeroom { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
