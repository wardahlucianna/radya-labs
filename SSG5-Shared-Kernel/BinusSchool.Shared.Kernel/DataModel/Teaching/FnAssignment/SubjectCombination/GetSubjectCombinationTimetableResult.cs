using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.SubjectCombination
{
    public class GetSubjectCombinationTimetableResult
    {
        public string Id { get; set; }
        public SubjectVm Subject { get; set; }
        public int TotalSession { get; set; }
        public CodeView Class { get; set; }
        public CodeView AcadYear { get; set; }
        public CodeView Level { get; set; }
        public CodeView Grade { get; set; }
        public CodeView Department { get; set; }
        public CodeView Streaming { get; set; }
    }

    public class SubjectVm : ItemValueVm
    {
        public string SubjectName { get; set; }
        public string SubjectId { get; set; }
        public int MaxSession { get; set; }
    }

    public class CodeView : CodeVm
    {
        public string Id { get; set; }
        public string IdMapping { get; set; }
    }

    public class TeacherVm : CodeWithIdVm
    {
        public string BinusianId { get; set; }
        public int TotalLoad { get; set; }
    }
}
