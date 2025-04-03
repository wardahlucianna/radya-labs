namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan
{
    public class GetLessonPlanSummaryDetailRequest
    {
        public string IdWeekSettingDetail { get; set; }
        public string IdSubject { get; set; }
        public string IdLessonTeacher { get; set; }
        public string IdUser { get; set; }
        public string IdAcademicYear { get; set; }
        public string PositionCode { get; set; }
    }
}
