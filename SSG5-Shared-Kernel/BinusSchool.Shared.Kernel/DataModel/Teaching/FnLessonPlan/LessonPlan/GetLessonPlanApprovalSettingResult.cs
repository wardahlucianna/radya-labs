using BinusSchool.Common.Model;
namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan
{
    public class GetLessonPlanApprovalSettingResult
    {
        public string IdLevelApproval { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public bool IsApprovalRequired { get; set; }
        public bool IsChangeable { get; set; }
    }
}