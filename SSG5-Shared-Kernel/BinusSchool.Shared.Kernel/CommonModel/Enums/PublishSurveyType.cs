using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BinusSchool.Common.Model.Enums
{
    public enum PublishSurveyType
    {
        [Description("Evaluating School Executive")]
        EvaluatingSchoolExecutive,
        [Description("Evaluating School")]
        EvaluatingSchool,
        [Description("Evaluating Tecahing And Non Teaching Staff")]
        EvaluatingTecahingAndNonTeachingStaff,
    }
}
