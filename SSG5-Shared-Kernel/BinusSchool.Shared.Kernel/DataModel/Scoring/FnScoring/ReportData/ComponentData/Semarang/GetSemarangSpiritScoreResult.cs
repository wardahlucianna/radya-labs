using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData.Semarang
{
    public class GetSemarangSpiritScoreResult
    {
        public string HtmlOutput { get; set; }
    }

    public class GetSemarangECYSpiritScoreResult_ScoreGrading
    {
        public decimal Score { get; set; }
        public string Grading { get; set; }
    }
}
