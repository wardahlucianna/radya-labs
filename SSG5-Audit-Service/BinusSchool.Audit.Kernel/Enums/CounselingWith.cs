using System.ComponentModel;

namespace BinusSchool.Audit.Kernel.Enums
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
