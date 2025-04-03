using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparation
{
    public class GetTextbookPreparationSubjectRequest
    {
        public string IdSubjectGroup { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
    }
}
