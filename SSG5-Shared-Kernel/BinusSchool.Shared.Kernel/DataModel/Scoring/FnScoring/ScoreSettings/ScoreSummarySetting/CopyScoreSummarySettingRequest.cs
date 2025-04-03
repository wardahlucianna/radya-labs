using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreSummarySetting
{
    public class CopyScoreSummarySettingRequest
    {
        public string IdSchool { get; set; }
        public string PreviousIdAcademicYear { get; set; }
        public string TargetIdAcademicYear { get; set; }
    }
}
