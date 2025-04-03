using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparationSubjectGroup
{
    public class AddTextbookPreparationSubjectGroupRequest
    {
        public string IdAcademicYear { get; set; }
        public string SubjectGroupName { get; set; }
        public List<string> IdSubject { get; set; }
    }
}
