using System.ComponentModel;

namespace BinusSchool.Audit.Kernel.Enums
{
    public enum PublishSurveyOption
    {
        All,
        Position,
        [Description("Specific User")]
        SpecificUser,
        Department,
        Grade,
        Level
    }
}
