using System.ComponentModel;

namespace BinusSchool.Audit.Kernel.Enums
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
