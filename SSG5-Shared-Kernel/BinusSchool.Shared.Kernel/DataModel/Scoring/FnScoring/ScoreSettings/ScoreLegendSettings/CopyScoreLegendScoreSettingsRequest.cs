using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreLegendSettings
{
    public class CopyScoreLegendScoreSettingsRequest
    {
        public string IdAcademicYearFrom { set; get; }  
        public string IdAcademicYearTo { set; get; }
        public string IdUser { set; get; }
    }
}
