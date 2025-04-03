using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.GeneralSettings.ScoreOptionSettings
{
    public class GetScoreOptionsResult : CodeWithIdVm
    {
        public string description { get; set; }
        public bool CurentStatus { get; set; }
        public string Option { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        
        public List<GetScoreOptionsDetailResult> ScoreOptionDetail { get; set; }

    }
    public class GetScoreOptionsDetailResult : CodeWithIdVm
    {
        public Int16 Key { get; set; }
        public string ShortDesc { get; set; }
    }

}
