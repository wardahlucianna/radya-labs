using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreLegendSettings
{
    public class UpdateScoreLegendSettingsRequest
    {
        public string IdScoreLegend { set; get; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
        public List<UpdateScoreLegendSettings_ScoreVm> ScoreList { set; get; }
    }
    public class UpdateScoreLegendSettings_ScoreVm
    {
        public string IdScoreLegendDetail { get; set; }
        public decimal Min { set; get; }
        public decimal Max { set; get; }
        public string Grade { set; get; }
        public string Description { set; get; }
        public string ConvertScore { set; get; }
    }
}
