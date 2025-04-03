using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparationSubjectGroup
{
    public class DetailTextbookPreparationSubjectGroupResult
    {
        public string Id {get; set; }
        public NameValueVm AcademicYear { get; set; }
        public string SubjectGroupName { get; set; }
        public List<TextbookPreparationSubject> Subject { get; set; }
    }

    public class TextbookPreparationSubject
    {
        public NameValueVm Subject { get; set; }
        public NameValueVm Level { get; set; }
        public NameValueVm Grade { get; set; }
    }
}
