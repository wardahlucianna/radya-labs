using System.ComponentModel;

namespace BinusSchool.Teaching.Kernel.Enums
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
