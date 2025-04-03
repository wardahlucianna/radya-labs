using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BinusSchool.Common.Model.Enums
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
