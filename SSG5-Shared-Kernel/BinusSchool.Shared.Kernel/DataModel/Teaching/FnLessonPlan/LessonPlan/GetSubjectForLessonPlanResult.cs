using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan
{
    public class GetSubjectForLessonPlanResult : CodeWithIdVm
    {
        public string SubjectLevel { get; set; }
        public string IdSubjectMappingSubjectLevel { get; set; }
    }
}
