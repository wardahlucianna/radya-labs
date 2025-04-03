using System.ComponentModel;
namespace BinusSchool.Common.Model.Enums
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
