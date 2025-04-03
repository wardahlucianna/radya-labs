using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ComponentDescription
{
    public class GetComponentDescriptionDetailResult
    {
        public string IdComponent { get; set; }
        public string ComponentName { get; set; }
        public List<GetComponentDescriptionDetailResult_Component_Score> Scores { get; set; }
    }

    public class GetComponentDescriptionDetailResult_Component_Score
    {
        public string IdComponentScoreGradeComment { get; set; }
        public string MinScore { get; set; }
        public string MaxScore { get; set; }
        public string Description { get; set; }
    }
}
