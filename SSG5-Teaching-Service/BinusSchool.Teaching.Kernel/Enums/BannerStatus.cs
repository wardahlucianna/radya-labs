using System.ComponentModel;

namespace BinusSchool.Teaching.Kernel.Enums
{
    public enum BannerStatus
    {
        [Description("Active")]
        Active,
        [Description("Upcoming")]
        Upcoming,
        [Description("Expired")]
        Expired,
    }
}
