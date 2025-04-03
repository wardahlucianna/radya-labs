using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData.Bekasi
{
    public class GetBekasiNatcurK1andK2Result
    {
        public string HtmlOutput { get; set; }
        public List<string> GenerateStatus { get; set; }
    }

    public class GetBekasiNatcurK1andK2Result_Score
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public int OrderNo { get; set; }
        public decimal? Score { get; set; }
        public string? Grading { get; set; }
        public string? GradingDescription { get; set; }
    }
}
