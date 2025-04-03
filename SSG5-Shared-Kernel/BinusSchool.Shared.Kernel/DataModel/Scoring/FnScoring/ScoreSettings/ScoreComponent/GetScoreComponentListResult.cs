using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreComponent
{
    public class GetScoreComponentListResult
    {
        public ItemValueVm Period { get; set; }
        public ItemValueVm Component { get; set; }
        public ItemValueVm SubComponent { get; set; }
        public decimal Weight { get; set; }
        public int CounterCount { get; set; }
    }
}
