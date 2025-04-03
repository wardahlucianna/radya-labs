using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.GeneralSettings.ScoreOptionSettings
{
    public class GetScoreOptionSettingsListResult
    {
        public string Description { get; set; }
        public string InputType { get; set; }
        public string Option { get; set; }
        public bool CurrentStatus { get; set; }
        public bool IsDelete { get; set; }
        public string Id { get; set; }
    }

}
