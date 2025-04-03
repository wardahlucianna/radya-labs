using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.StudentParentScoreView
{
    public class GetScoreViewPeriodDetailRequest : CollectionRequest
    {
        public string IdScoreViewPeriod { get; set; }
    }
}
