namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherPrivilege
{
    public class GetTeacherPrivilegeRequest
    {
        public string UserId { get; set; }
        public string IdSchool { get; set; }
        public string IdSchoolAcademicYear { get; set; }
        public int? Semester { get; set; }
    }
}
