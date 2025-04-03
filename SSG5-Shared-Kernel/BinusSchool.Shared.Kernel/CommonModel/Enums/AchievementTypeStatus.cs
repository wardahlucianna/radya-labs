using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BinusSchool.Common.Model.Enums
{
    public enum AchievementTypeStatus
    {
        [Description("Waiting Approval")]
        WaitingApproval,
        Approved,
        Declined,
        [Description("Delete Requested")]
        DeleteRequested,
        Deleted
    }
}
