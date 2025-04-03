
namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetSummaryByPeriodRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdUser { get; set; }
        public string IdSchool { get; set; }
        public string SelectedPosition { get; set; }
    }
}
