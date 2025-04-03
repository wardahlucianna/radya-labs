using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BinusSchool.Common.Model.Enums
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
