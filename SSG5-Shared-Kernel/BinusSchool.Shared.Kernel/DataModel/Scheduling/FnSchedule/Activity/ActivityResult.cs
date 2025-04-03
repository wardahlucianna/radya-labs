using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Activity
{
    public class ActivityResult : CodeWithIdVm
    {
        public CodeWithIdVm School { get; set; }
        public bool IsUsed { get; set; }
    }
}
