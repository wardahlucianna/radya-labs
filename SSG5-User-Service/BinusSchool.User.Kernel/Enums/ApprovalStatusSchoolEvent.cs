using System.ComponentModel;

namespace BinusSchool.User.Kernel.Enums
{
    public enum ApprovalStatusSchoolEvent
    {
        [Description("On Review (1)")]
        OnReview1,
        [Description("On Review (2)")]
        OnReview2,
        Approved,
        Declined
    }
}
