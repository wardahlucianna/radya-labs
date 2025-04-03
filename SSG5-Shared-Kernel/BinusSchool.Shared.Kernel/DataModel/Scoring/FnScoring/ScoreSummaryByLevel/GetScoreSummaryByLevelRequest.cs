using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByLevel
{
    public class GetScoreSummaryByLevelRequest
    {
        public string IdAcademicYear { set; get; }
        public int Semester { set; get; }
        public string IdSchool { set; get; }
    }
}
