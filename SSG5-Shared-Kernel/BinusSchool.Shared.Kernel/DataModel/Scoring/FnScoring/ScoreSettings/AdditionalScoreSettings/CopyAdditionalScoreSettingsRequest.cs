using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.AdditionalScoreSettings
{
    public class CopyAdditionalScoreSettingsRequest
    {
        public string IdUser { set; get; }
        public string AcademicYearId { set; get; }
        public string PreviousAcademicYearId { set; get; }
    }
}
