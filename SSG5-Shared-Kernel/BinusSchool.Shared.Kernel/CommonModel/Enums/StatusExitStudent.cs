
using System.ComponentModel;

namespace BinusSchool.Common.Model.Enums
{
    public enum StatusExitStudent
    {
        [Description("Waiting Approval")]
        WaitingApproval,
        [Description("Approved")]
        Approved,
        [Description("Approved With Notes")]
        ApproveWithNote,
        [Description("Cancelled by Parent")]
        CancelledByParent,
        [Description("Cancelled by School")]
        CancelledBySchool,
        [Description("Delete Request")]
        DeleteRequest
    }
}
