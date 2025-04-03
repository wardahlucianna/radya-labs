using System.Collections.Generic;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.SubjectCombination
{
    public class GetListSubjectCombinationTimetableResult
    {
        public string Id { get; set; }
        public SubjectVm Subject { get; set; }
        public int TotalSession { get; set; }
        public CodeView Class { get; set; }
        public CodeView AcadYear { get; set; }
        public CodeView Level { get; set; }
        public CodeView Grade { get; set; }
        public CodeView Department { get; set; }
        public List<CodeView> Streaming { get; set; }
        public CodeView Division { get; set; }
        public CodeView Term { get; set; }
    }

   
}
