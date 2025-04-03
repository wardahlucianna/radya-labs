using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.SubjectAliasSettings
{
    public class CopySubjectAliasSettingsRequest
    {
        public string IdUser { get; set; }
        public string AcademicYearId { get; set; }
        public string PreviousAcademicYearId { get; set; }
    }
}
