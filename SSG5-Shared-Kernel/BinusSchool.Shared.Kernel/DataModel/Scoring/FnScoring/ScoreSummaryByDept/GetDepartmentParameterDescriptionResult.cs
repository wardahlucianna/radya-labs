using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByDept
{
    public class GetDepartmentParameterDescriptionResult
    {
        public string School { set; get; }
        public string AcademicYear { set; get; }
        public int Semester { set; get; }
        public string Level { set; get; }
        public string Grade { set; get; }
        public string  Department { get; set; }
    }
}
