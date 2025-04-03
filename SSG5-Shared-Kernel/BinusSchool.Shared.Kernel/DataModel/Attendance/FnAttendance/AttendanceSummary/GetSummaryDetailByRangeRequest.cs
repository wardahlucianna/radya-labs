using System;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetSummaryDetailByRangeRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string IdUser { get; set; }
        public string SelectedPosition { get; set; }
        public string ClassId { get; set; }
        public string IdSession { get; set; }
    }
}
