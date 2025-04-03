using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BinusSchool.Common.Model.Enums
{
    public enum CounselingWith
    {
        Mother,
        Father,
        Student,
        [Description("Both Parent")]
        BothParent,
    }
}
