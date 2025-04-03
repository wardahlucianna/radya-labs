namespace BinusSchool.Attendance.FnAttendance.Models
{
    public class ScheduleResult
    {
        public string IdLesson { get; set; }
        public TeacherResult Teacher { get; set; }
        public string IdWeek { get; set; }
        public string IdDay { get; set; }
        public string IdSession { get; set; }
    }
}
