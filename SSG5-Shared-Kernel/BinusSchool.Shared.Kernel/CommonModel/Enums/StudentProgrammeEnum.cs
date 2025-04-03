using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
namespace BinusSchool.Common.Model.Enums
{
    public enum StudentProgrammeEnum
    {
        [Description("IB Diploma")]
        IbDiploma,
        [Description("IB Course")]
        IbCourse,
        [Description("HS Diploma")]
        HsDiploma,
    }
}
