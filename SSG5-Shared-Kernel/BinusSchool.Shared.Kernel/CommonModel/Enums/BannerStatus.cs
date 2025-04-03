using System.ComponentModel;

namespace BinusSchool.Common.Model.Enums
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
