using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan
{
    public class GetDetailLessonPlanDocumentRequest : CollectionRequest
    {
        public string IdLessonPlanDocument { get; set; }
    }
}