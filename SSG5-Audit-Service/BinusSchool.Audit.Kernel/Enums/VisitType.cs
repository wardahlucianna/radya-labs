﻿using System.ComponentModel;

namespace BinusSchool.Audit.Kernel.Enums
{
    public enum VisitType
    {
        [Description("Reguler Visitor")]
        RegulerVisitor,
        [Description("Invitation Booking")]
        InvitationBooking,
        [Description("Personal Invitation")]
        PersonalInvitation
    }
}
