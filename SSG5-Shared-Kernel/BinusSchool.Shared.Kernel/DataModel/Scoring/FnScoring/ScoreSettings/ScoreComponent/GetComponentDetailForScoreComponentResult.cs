using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreComponent
{
    public class GetComponentDetailForScoreComponentResult
    {
        public ItemValueVm Period { get; set; }
        public string IdComponent { get; set; }
        public string ShortDesc { get; set; }
        public string Description { get; set; }
        public int OrderNo { get; set; }
        public GetComponentDetail_ScoreLegend? ScoreLegend { get; set; }
        public bool AverageSubComponentScore { get; set; }
        public bool CalculateWeightFromSubComponent { get; set; }
        public bool ShowGradingAsScore { get; set; }
        public bool CanDeleteComponent { get; set; }
        public decimal? Weight { get; set; }
        public bool IsLearningObjective { get; set; }
    }

    public class GetComponentDetail_ScoreLegend
    {
        public int? Id { get; set; }
        public string Description { get; set; }
    }
}
