using System.ComponentModel;

namespace BinusSchool.Scheduling.Kernel.Enums
{
    public enum DocumentRequestApprovalStatus
    {
        [Description("Waiting for Approval")]
        WaitingForApproval = 1,

        [Description("Approved")]
        Approved,

        [Description("Declined")]
        Declined,

    }
}
