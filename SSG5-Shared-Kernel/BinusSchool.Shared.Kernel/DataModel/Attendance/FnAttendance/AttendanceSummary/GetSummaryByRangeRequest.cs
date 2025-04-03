using System;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetSummaryByRangeRequest
    {
        public string IdAcademicYear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IdUser { get; set; }
        public string IdSchool { get; set; }
        public string SelectedPosition { get; set; }
    }
}
