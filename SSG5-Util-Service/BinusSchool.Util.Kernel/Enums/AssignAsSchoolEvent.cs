using System.ComponentModel;

namespace BinusSchool.Util.Kernel.Enums
{
    public enum AssignAsSchoolEvent
    {
        [Description("Event Coordinator")]
        EventCoordinator,
        [Description("Attendance PIC")]
        AttendancePIC,
        Registrant,
        Approver,
        [Description("Event Creator")]
        EventCreator,
        ActivityPIC,
    }
}
