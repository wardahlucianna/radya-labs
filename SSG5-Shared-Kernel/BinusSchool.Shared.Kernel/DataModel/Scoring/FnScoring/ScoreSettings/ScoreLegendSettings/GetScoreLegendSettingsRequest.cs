using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreLegendSettings
{
    public class GetScoreLegendSettingsRequest : CollectionRequest
    {
        public string IdAcademicYear { set; get; }
        public string IdLevel { set; get; }
    }
}
