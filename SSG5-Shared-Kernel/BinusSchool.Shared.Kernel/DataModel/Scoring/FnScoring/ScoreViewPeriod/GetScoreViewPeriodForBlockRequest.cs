using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreViewPeriod
{
    public class GetScoreViewPeriodForBlockRequest
    {
        public string UserId { set; get; }
        public string IdAcademicYear { set; get; }
        public int Semester { set; get; }
    }
}
