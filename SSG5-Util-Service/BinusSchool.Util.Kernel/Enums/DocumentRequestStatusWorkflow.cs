using System.ComponentModel;

namespace BinusSchool.Util.Kernel.Enums
{
    public enum DocumentRequestStatusWorkflow
    {
        [Description("Initial Status (Temporary)")]
        InitialStatus = 0,

        [Description("Waiting for Approval")]
        WaitingForApproval,

        [Description("Declined")]
        Declined,

        [Description("Waiting for Payment")]
        WaitingForPayment,

        [Description("Waiting for Payment Verification")]
        WaitingForPaymentVerification,

        [Description("On Process")]
        OnProcess,

        [Description("Finished")]
        Finished,

        [Description("Collected")]
        Collected,

        [Description("Canceled")]
        Canceled
    }
}
