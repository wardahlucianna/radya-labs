using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan
{
    public class GetLessonPlanApproverSettingResult : CodeWithIdVm
    {
        public string IdSchool { get; set; }
        public CodeWithIdVm Role { get; set; }
        public CodeWithIdVm Position { get; set; }
        public CodeWithIdVm User { get; set;}
    }
}
