namespace BinusSchool.Persistence.AttendanceDb.Models
{
    public class GradeDto : LevelDto
    {
        public string IdGrade { get; set; }
        public string GradeName { get; set; }
        public int GradeOrder { get; set; }
    }
}
