using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetStudentWorkhabitDetailRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdStudent { get; set; }
        public string IdWorkhabit { get; set; }
    }
}
