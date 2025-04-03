using System.ComponentModel;

namespace BinusSchool.Workflow.Kernel.Enums
{
    public enum ApprovalTypeScoring
    {
        [Description("Score")]
        UpdateScore = 1,
        [Description("Progress Status")]
        UpdateProgressStatus,
        [Description("Teacher Comment")]
        UpdateTeacherComment,
        [Description("Subject Mapping")]
        UpdateSubjectMapping,
    }
}
