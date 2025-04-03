using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData.Bekasi
{
    public class GetBekasiSpiritScoreResult
    {
        public string HtmlOutput { get; set; }
    }

    public class GetBekasiECYSpiritScoreResult_ScoreGrading
    {
        public decimal Score { get; set; }
        public string Grading { get; set; }
    }
}
