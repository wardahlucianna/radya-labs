using System.ComponentModel;

namespace BinusSchool.Workflow.Kernel.Enums
{
    public enum EventAttendanceType
    {
        [Description("Only for Holiday event type")]
        NotSet,

        [Description("Mandatory Attendance")]
        Mandatory,

        [Description("No Attendance")]
        None,

        [Description("All Present")]
        All,

        [Description("All Excuse Absence")]
        AllExcuse
    }
}
