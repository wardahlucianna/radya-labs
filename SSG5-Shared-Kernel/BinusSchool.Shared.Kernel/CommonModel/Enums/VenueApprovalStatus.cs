using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BinusSchool.Common.Model.Enums
{
    public enum VenueApprovalStatus
    {
        [Description("Initial Status (Temporary)")]
        InitialStatus = 0,

        [Description("Approved")]
        Approved,

        [Description("Rejected")]
        Rejected,

        [Description("Waiting for Approval")]
        WaitingForApproval,

        [Description("No Need Approval")]
        NoNeedApproval,

        [Description("Cancelled")]
        Canceled
    }
}
