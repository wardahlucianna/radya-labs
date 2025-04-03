using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping
{
    public class GetClassroomMapByGradeResult : CodeWithIdVm
    {
        public string Formatted { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public ClassroomMapPathway Pathway { get; set; }
        public CodeWithIdVm Class { get; set; }
    }

    public class ClassroomMapPathway
    {
        public string Id { get; set; }
        public IEnumerable<CodeWithIdVm> PathwayDetails { get; set; }
    }
}
