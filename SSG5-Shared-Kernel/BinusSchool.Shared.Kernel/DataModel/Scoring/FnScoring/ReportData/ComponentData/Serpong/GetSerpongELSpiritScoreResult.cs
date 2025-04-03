using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData.Serpong
{
    public class GetSerpongELSpiritScoreResult
    {
        public string HtmlOutput { get; set; }
        public List<string> GenerateStatus { get; set; }
    }

    public class GetSerpongELSpiritScoreResult_ScoreGrading
    {
        public decimal Score { get; set; }
        public string Grading { get; set; }
    }
}
