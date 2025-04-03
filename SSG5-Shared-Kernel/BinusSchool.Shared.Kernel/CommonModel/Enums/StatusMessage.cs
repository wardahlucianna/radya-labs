using System.ComponentModel;

namespace BinusSchool.Common.Model.Enums
{
    public enum StatusMessage
    {
        WaitingApprove1 = 1,
        WaitingApprove2,
        Approved,
        Rejected,
        OnProgress,
        WaitingUnsendApprove1,
        WaitingUnsendApprove2,
        [Description("Edit Rejected")]
        EditRejected,
        [Description("Unsend Rejected")]
        UnsendRejected
    }
}
