﻿using System.ComponentModel;

namespace BinusSchool.School.Kernel.Enums
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
