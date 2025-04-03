using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectSplitMapping
{
    public class GetSubjectSplitMappingResult : CodeWithIdVm
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public string Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public string IdParentSubject { get; set; }
        public string ParentSubject { get; set; }
        public List<string> ChildSubjectList { get; set; }      
    }
}
