using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparationSubjectGroup
{
    public class GetTextbookPreparationSubjectResult : CodeWithIdVm
    {
        public string IdLevel { get; set; }
        public string Level { get; set; }
        public string IdGrade { get; set; }
        public string Grade { get; set; }
        public string Subject { get; set; }
    }
}
