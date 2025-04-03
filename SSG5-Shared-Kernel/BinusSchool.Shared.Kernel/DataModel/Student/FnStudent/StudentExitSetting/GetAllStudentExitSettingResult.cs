namespace BinusSchool.Data.Model.Student.FnStudent.StudentExitSetting
{
    public class GetAllStudentExitSettingResult
    {
        public string Id { get; set; }
        public string IdAcademicYear { get; set; }
        public string AcademicYear { get; set; }
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public int Semester { get; set; }
        public string? LevelCode { get; set; }
        public string? GradeCode { get; set; }
        public string? HomeroomName { get; set; }
        public bool IsExit { get; set; }
    }
}
