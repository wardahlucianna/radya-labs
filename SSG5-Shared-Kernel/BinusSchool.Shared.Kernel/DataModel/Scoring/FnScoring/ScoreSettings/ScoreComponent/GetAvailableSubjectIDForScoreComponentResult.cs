using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreComponent
{
    public class GetAvailableSubjectIDForScoreComponentResult
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public List<ItemValueVm> ClassIds { get; set; }
    }
}
