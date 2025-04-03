using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreLegendSettings
{
    public class AddScoreLegendSettingsRequest
    {
        public string IdLevel { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
        public List<AddScoreLegendSettings_ScoreVm> ScoreList { set; get; }
    }

    public class AddScoreLegendSettings_ScoreVm
    {
        public decimal Min { set; get; }
        public decimal Max { set; get; }
        public string Grade { set; get; }
        public string Description { set; get; }
        public string ConvertScore { set; get; }
    }
}
