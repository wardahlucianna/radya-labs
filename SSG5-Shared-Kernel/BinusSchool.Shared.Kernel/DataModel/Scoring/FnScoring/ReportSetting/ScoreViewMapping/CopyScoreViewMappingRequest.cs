using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportSetting.ScoreViewMapping
{
    public class CopyScoreViewMappingRequest
    {
        public string IdSchool { set; get; }
        public string IdAcademicYearSource { set; get; }
        public string IdAcademicYearTarget { set; get; }
    }
}
