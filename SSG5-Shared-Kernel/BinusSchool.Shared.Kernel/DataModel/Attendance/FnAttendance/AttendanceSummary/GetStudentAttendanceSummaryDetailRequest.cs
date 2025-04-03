using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetStudentAttendanceSummaryDetailRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdStudent { get; set; }
        public string IdAttendance { get; set; }
        public string PeriodType { get; set; }
    }
}
