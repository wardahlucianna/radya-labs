using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan
{
    public class GetLessonPlanSummaryRequest : CollectionRequest
    {
        public string IdUser { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdPeriod { get; set; }
        public string IdSubject { get; set; }
        public string PositionCode { get; set; }

    }
}
