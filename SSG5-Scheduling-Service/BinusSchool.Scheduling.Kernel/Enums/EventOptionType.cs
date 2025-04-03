using System.ComponentModel;

namespace BinusSchool.Scheduling.Kernel.Enums
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
