using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BinusSchool.Common.Model.Enums
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
