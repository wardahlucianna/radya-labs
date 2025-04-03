namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan
{
    public class GetSubjectForLessonPlanRequest
    {
        public string IdUser { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public string Search { get; set; }
    }
}