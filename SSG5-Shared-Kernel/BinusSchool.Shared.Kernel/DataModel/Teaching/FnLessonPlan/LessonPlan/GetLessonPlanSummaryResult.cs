using System.Collections.Generic;

namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan
{
    public class GetLessonPlanSummaryResult
    {
        public string IdAcademicYear { get; set; }
        public string AcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string Level { get; set; } 
        public string IdGrade { get; set; }
        public string Grade { get; set; }
        public string Subject { get; set; }
        public string IdTerm { get; set; }
        public string Term { get; set; }
        public string IdLessonPlan { get; set; }
        public string IdLessonTeacher { get; set; }
        public string IdSubject { get; set; }
        public string ClassId { get; set; }
        public List<GetWeekResult> Week { get; set; }
    }

    public class GetWeekResult
    {
        public string IdWeekSettingDetail { get; set; }
        public int WeekNumber { get; set; }
        public string Text { get; set; }
        public string PositionCode { get; set; }
    }

    public class GetWeekQueryResult
    {
        public string IdWeekSettingDetail { get; set; }
        public int WeekNumber { get; set; }
        public int Uploaded { get; set; }
        public int Total { get; set; }
        public string PositionCode { get; set; }
    }
}
