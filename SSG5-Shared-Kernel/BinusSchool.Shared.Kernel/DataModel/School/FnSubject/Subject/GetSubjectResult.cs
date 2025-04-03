using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSubject.Subject
{
    public class GetSubjectResult : CodeWithIdVm
    {
        public string Acadyear { get; set; }
        public string Grade { get; set; }
        public string Level { get; set; }
        public string Pathway { get; set; }
        public string IdDepartment { get; set; }
        public string Department { get; set; }
        public string CurriculumType { get; set; }
        public string SubjectType { get; set; }
        public string SubjectId { get; set; }
        public string SubjectLevel { get; set; }
        public IEnumerable<SubjectLevelResult> SubjectLevels { get; set; }
        public CodeWithIdVm SubjectGroup { get; set; }
        public int MaxSession { get; set; }
    }

    public class SubjectLevelResult : CodeWithIdVm
    {
        public bool IsDefault { get; set; }
    }
}
