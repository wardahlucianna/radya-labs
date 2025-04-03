using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparationSubjectGroup
{
    public class GetTextbookPreparationSubjectGroupResult : CodeWithIdVm
    {
        public string Id { get; set; }
        public string AcademicYear { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string SubjectGroup { get; set; }
        public string Subject { get; set; }
        public bool IsDisabledEdit { get; set; }
        public bool IsDisabledDelete { get; set; }
    }
}
