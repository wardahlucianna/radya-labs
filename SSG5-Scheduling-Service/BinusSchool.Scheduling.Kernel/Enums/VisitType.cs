using System.ComponentModel;

namespace BinusSchool.Scheduling.Kernel.Enums
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
