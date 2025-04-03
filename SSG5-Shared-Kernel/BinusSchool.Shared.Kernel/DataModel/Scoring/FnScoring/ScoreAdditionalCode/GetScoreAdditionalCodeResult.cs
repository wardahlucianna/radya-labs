using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreAdditionalCode
{
    public class GetScoreAdditionalCodeResult
    {
        public string IdScoreAdditionalCode { get; set; }
        public string IdSchool { get; set; }
        public decimal Key { get; set; }
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
        public bool CurrentStatus { get; set; }
    }
}
