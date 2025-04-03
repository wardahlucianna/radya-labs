using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreViewPeriod
{
    public class GetScoreViewPeriodForBlockResult
    {
        public bool IsBlockPeriod { set; get; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
