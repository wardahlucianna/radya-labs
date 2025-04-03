namespace BinusSchool.Attendance.FnAttendance.Models
{
    public class LessonTeacherResult
    {
        public string IdLesson { set; get; }
        public string IdUserTeacher { set; get; }
        public string ClassId { set; get; }
        public bool IsAttendance { set; get; }
    }
}
