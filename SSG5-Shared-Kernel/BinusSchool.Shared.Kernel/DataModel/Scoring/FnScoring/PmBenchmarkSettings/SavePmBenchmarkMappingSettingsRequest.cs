using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.PmBenchmarkSettings
{
    public class SavePmBenchmarkMappingSettingsRequest
    {
        public string IdAssessmentComponentSetting { get; set; }
        public string IdSchool { get; set; }
        public string PeriodCode { get; set; }
        public List<string>? IdGrades { get; set; }
        public string ComponentName { get; set; }
        public int OrderNumber { get; set; }
        public string Description { get; set; }
        public string IdScoreOption { get; set; }
    }
}
