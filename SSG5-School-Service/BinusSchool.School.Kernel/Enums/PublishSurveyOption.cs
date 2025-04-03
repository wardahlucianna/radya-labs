using System.ComponentModel;

namespace BinusSchool.School.Kernel.Enums
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
