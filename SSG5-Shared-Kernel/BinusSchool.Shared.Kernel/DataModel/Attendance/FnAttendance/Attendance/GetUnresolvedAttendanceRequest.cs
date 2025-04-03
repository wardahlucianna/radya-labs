
namespace BinusSchool.Data.Model.Attendance.FnAttendance.Attendance
{
    public class GetUnresolvedAttendanceRequest
    {
        public string IdSchool { get; set; }
        public string IdUser { get; set; }
        public bool IsStaff { get; set; }
    }
}
