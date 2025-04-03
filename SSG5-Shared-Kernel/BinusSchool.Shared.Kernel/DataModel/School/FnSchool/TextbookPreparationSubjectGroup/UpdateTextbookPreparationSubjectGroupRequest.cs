using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparationSubjectGroup
{
    public class UpdateTextbookPreparationSubjectGroupRequest
    {
        public string Id { get; set; }
        public string SubjectGroupName { get; set; }
        public List<string> IdSubject { get; set; }
    }
}
