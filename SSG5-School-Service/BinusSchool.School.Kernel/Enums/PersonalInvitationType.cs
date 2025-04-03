using System.ComponentModel;

namespace BinusSchool.School.Kernel.Enums
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
