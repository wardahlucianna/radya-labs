using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData
{
    public class GetElectivesReportResult
    {
        public string HtmlOutput { get; set; }
        public List<string> GenerateStatus { get; set; }
    }

    public class GetElectivesReportResult_Attandance
    {
        public string AttSmt1 { get; set; }
        public string Att2DecimalSmt1 { get; set; }
        public string AttWithPercentSmt1 { get; set; }
        public string Att2DecimalWithPercentSmt1 { get; set; }
        public string AttSmt2 { get; set; }
        public string Att2DecimalSmt2 { get; set; }
        public string AttWithPercentSmt2 { get; set; }
        public string Att2DecimalWithPercentSmt2 { get; set; }
        public string ScorePerformanceSmt1 { get; set; }
        public string ScorePerformanceSmt2 { get; set; }
    }
}
