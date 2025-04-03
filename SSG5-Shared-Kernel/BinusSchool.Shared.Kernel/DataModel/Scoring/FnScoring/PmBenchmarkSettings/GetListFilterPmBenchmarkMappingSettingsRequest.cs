using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.PmBenchmarkSettings
{
    public class GetListFilterPmBenchmarkMappingSettingsRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public string PeriodCode { get; set; }
        public bool ShowLevel { get; set; }
        public bool ShowGrade { get; set; }
        public bool ShowPeriod { get; set; }
        public bool ShowComponent { get; set; }
    }
}
