using System.ComponentModel;

namespace BinusSchool.Util.Kernel.Enums
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
