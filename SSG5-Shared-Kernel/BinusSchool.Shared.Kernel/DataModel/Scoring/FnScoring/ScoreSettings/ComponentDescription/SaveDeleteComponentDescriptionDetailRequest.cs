using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ComponentDescription
{
    public class SaveDeleteComponentDescriptionDetailRequest
    {
        public string IdComponent { get; set; }
        public string ComponentName { get; set; }
        public List<SaveDeleteComponentDescriptionDetailRequest_Scores> Scores { get; set; }
    }

    public class SaveDeleteComponentDescriptionDetailRequest_Scores
    {
        public string IdComponentScoreGradeComment { get; set; }
        public string MinScore { get; set; }
        public string MaxScore { get; set; }
        public string Description { get; set; }
    }
}
