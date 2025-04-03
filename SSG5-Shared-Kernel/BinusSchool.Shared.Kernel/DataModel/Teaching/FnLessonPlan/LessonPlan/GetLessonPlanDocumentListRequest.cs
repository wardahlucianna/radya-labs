namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan
{
    public class GetLessonPlanDocumentListRequest
    {
        public string IdUser { get; set; }
        public string IdSchool { get; set; }
        public string IdLessonPlan { get; set; }
        public string IdLesson { get; set; }
    }
}
