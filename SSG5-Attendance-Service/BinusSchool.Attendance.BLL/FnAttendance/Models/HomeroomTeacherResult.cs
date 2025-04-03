using BinusSchool.Common.Model;

namespace BinusSchool.Attendance.FnAttendance.Models
{
    public class HomeroomTeacherResult
    {
        public TeacherResult Teacher { get; set; }
        public string IdHomeroom { get; set; }
        public string IdGrade { get; set; }
        public string IdClassroom { get; set; }
        public CodeWithIdVm Position { get; set; }
        public bool IsAttendance { get; set; }
    }
}
