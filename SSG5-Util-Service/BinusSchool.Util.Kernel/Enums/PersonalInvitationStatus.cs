using System.ComponentModel;

namespace BinusSchool.Util.Kernel.Enums
{
    public enum PersonalInvitationStatus
    {
        [Description("On Request")]
        OnRequest,
        Approved,
        Cancelled,
        Declined,
        [Description("No Response")]
        NoResponse,
        [Description("No Approval")]
        NoApproval
    }
}
