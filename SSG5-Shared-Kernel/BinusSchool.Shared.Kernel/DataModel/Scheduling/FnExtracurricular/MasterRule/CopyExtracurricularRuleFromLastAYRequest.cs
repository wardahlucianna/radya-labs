using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterRule
{
    public class CopyExtracurricularRuleFromLastAYRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public bool IsBothSemester { get; set; }
        public List<string> IdExtracurricularRule { get; set; }
    }
}
