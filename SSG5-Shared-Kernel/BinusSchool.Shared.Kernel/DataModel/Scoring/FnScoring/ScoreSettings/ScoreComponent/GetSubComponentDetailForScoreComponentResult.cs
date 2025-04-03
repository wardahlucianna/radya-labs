using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreComponent
{
    public class GetSubComponentDetailForScoreComponentResult
    {
        public ItemValueVm Period { get; set; }
        public string IdSubComponent { get; set; }
        public ItemValueVm Component { get; set; }
        public string ShortDesc { get; set; }
        public string Description { get; set; }
        public decimal Weight { get; set; }
        public ItemValueVm ScoreOption { get; set; }
        public int OrderNo { get; set; }
        public bool AverageCounterScore { get; set; }
        public bool CanDeleteSubComponent { get; set; }
    }
}
