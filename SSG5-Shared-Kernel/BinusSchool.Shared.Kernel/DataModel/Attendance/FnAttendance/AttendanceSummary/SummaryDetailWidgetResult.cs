
namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class SummaryDetailWidgetResult
    {
        public int TotalStudent { get; set; }
        public int Unsubmitted { get; set; }
        public int Pending { get; set; }
        public int Present { get; set; }
        public int Late { get; set; }
        public int ExcusedAbsence { get; set; }
        public int UnexcusedAbsence { get; set; }
    }
}
