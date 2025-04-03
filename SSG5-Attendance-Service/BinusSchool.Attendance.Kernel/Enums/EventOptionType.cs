using System.ComponentModel;

namespace BinusSchool.Attendance.Kernel.Enums
{
    public enum EventOptionType
    {
        None,
        All,
        Grade,
        Department,
        Subject,
        [Description("Personal Event")]
        Personal,
        Position,
        Level
    }
}
