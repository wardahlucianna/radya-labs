using System.ComponentModel;

namespace BinusSchool.School.Kernel.Enums
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
