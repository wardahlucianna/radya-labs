﻿using System.ComponentModel;

namespace BinusSchool.Workflow.Kernel.Enums
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
