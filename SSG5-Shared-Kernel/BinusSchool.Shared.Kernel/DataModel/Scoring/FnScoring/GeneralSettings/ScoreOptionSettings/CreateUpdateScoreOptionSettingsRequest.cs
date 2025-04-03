using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.GeneralSettings.ScoreOptionSettings
{
    public class CreateUpdateScoreOptionSettingsRequest
    {
        public string IdScoreOption { get; set; }
        public string Description { get; set; }
        public string InputType { get; set; }
        public bool CurrentStatus { get; set; }
        public string IdSchool { get; set; }
        public decimal MinScoreLength { get; set; }
        public decimal MaxScoreLength { get; set; }
        public List<ListCreateUpdateScoreOptionSettingsRequest> ScoreOptionDetail { get; set; }
    }

    public class ListCreateUpdateScoreOptionSettingsRequest
    {
        public string IdScoreOptionDetail { get; set; }
        public Int16 Key { get; set; }
        public string Grade { get; set; }
        public string Description { get; set; }
    }
}
