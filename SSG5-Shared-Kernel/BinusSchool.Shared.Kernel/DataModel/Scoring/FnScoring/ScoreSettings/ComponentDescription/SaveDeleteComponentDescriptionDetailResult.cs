using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ComponentDescription
{
    public class SaveDeleteComponentDescriptionDetailResult
    {

    }
    
    public class SaveDeleteComponentDescriptionDetailResult_Scores
    {
        public string IdComponentScoreGradeComment { get; set; }
        public string MinScore { get; set; }
        public string MaxScore { get; set; }
        public string Description { get; set; }
    }
}
