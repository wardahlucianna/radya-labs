using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.PmBenchmarkSettings
{
    public class SavePmBenchmarkPeriodSettingsRequest
    {
        public string? IdAssessmentSetting { get; set; }
        public List<string> IdGrades { get; set; }
        public string PeriodCode { get; set; }
        public string IdSchool { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
