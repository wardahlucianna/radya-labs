using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BinusSchool.Common.Model.Enums
{
    public enum PersonalInvitationType
    {
        [Description("Academic Matters")]
        AcademicMatters,
        [Description("Affective Appointment")]
        AffectiveMatters,
        [Description("Genetal Appointment")]
        GeneralAppointment,
        Counselling,
    }
}
