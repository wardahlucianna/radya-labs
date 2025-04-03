using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.POISetting.POIMappingSettings
{
    public class GetPOIMappingSettingsRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
    }
}
