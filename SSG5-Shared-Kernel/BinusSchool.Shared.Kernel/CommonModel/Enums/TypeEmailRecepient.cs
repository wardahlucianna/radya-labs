using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
namespace BinusSchool.Common.Model.Enums
{
    public enum TypeEmailRecepient
    {
        [Description("Moving Student Enrollment")]
        MovingStudentEnrollment,
        [Description("Student Program")]
        StudentProgram,
        [Description("Moving Student Homeroom")]
        MovingStudentHomeroom,
    }
}
