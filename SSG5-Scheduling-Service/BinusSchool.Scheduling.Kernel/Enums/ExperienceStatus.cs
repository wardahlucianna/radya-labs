using System.ComponentModel;

namespace BinusSchool.Scheduling.Kernel.Enums
{
    public enum ExperienceStatus
    {
        All,
        [Description("To Be Determined")]
        ToBeDetermined,
        Approved,
        [Description("Need Revision")]
        NeedRevision,
        Completed
    }
}
