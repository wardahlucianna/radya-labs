using System.ComponentModel;

namespace BinusSchool.Attendance.Kernel.Enums
{
    public enum BLPFinalStatus
    {
        [Description("Not Applicable")]
        NotApplicable = -1,
        [Description("Not Allowed")]
        NotAllowed,
        [Description("Allowed")]
        Allowed,
        [Description("Allowed - Document Needed")]
        AllowedDocumentNeeded,
        [Description("Allowed - F2FL Permission Needed")]
        AllowedF2FLPermissionNeeded
    }
}
