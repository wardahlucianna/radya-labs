using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment.LHAndCA
{
    public class GetAssignLHAndCADetailResult : DetailResult2
    {
        public int MaxLoad { get; set; }
        public CodeWithIdVm Acadyear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public NonTeachingLoadVm LevelHeadLoad { get; set; }
        public NonTeachingLoadVm ClassAdvisorLoad { get; set; }
        public LevelHeadDetail LevelHead { get; set; }
        public IEnumerable<ClassAdvisorDetail> ClassAdvisors { get; set; }
    }

    public class LevelHeadDetail : ItemValueVm
    {
        public int LoadAfterAssigned { get; set; }
    }

    public class ClassAdvisorDetail : CodeWithIdVm
    {
        public ItemValueVm ClassAdvisor { get; set; }
        public ItemValueVm Pathway { get; set; }
        public IEnumerable<string> Status { get; set; }
        public int LoadAfterAssigned { get; set; }
    }

    public class NonTeachingLoadVm
    {
        public string Id { get; set; }
        public int Load { get; set; }
    }
}
