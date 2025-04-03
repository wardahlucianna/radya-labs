using System.ComponentModel;

namespace BinusSchool.Attendance.Kernel.Enums
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
