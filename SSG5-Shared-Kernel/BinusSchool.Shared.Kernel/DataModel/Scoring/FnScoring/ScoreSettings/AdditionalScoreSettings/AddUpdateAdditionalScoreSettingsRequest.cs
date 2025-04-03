using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.AdditionalScoreSettings
{
    public class AddUpdateAdditionalScoreSettingsRequest
    {
        public string IdScoreAdditionalCode { set; get; }
        public string IdSchool { set; get; }
        public string IdGrade { set; get; }
        public string Category { set; get; }
        public decimal Key { set; get; }
        public string LongDesc { set; get; }
        public bool ShowScoreAsNA { set; get; }
        public bool CurrentStatus { set; get; }
    }
}
