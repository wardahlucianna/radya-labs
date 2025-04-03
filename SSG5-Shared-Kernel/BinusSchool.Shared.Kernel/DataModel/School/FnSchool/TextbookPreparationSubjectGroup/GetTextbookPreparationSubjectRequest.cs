using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparationSubjectGroup
{
    public class GetTextbookPreparationSubjectRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public List<string> IdGrade { get; set; }
    }
}
