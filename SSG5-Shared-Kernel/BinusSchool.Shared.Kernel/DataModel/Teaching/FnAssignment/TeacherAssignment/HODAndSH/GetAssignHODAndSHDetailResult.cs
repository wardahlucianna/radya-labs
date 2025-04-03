using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment.HODAndSH
{
    public class GetAssignHODAndSHDetailResult
    {
        public string Id { get; set; }
        public ItemValueVm SchoolUser { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Department { get; set; }
        public List<SubjectVm> Subjects { get; set; }
        public List<SubjectHeadVm> SubjectHeads { get; set; }
        public SchoolNonTeachingLoadVm HOD { get; set; }
        public SchoolNonTeachingLoadVm SubjectHead { get; set; }
        public SchoolNonTeachingLoadVm SubjectHeadAssistance { get; set; }
    }

    public class SubjectVm : ItemValueVm
    {
        public string SubjectId { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
    }

    public class SubjectHeadVm
    {
        public ItemValueVm User { get; set; }
        public string IdSubject { get; set; }
        public string IdUserNonTeachingAcademic { get; set; }
        public SubjectHeadAssistanceVm SubjectHeadAssistance { get; set; }
    }

    public class SubjectHeadAssistanceVm
    {
        public ItemValueVm User { get; set; }
        public string IdSubject { get; set; }
        public string IdUserNonTeachingAcademic { get; set; }
    }

    public class SchoolNonTeachingLoadVm
    {
        public string Id { get; set; }
        public int Load { get; set; }
    }
}
