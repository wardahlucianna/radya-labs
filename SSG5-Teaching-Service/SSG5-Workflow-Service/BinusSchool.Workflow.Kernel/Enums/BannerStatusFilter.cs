using System.ComponentModel;

namespace BinusSchool.Workflow.Kernel.Enums
{

    public enum BannerStatusFilter
    {
        [Description("All")]
        All,
        [Description("Active")]
        Active,
        [Description("Upcoming")]
        Upcoming,
        [Description("Expired")]
        Expired,
    }
}
