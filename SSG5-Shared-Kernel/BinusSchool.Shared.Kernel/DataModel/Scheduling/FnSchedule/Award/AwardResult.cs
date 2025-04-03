using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Award
{
    public class AwardResult : CodeWithIdVm
    {
        public CodeWithIdVm School { get; set; }
        public bool IsUsed { get; set; }
        public bool IsRecommendation { get; set; }
    }
}
