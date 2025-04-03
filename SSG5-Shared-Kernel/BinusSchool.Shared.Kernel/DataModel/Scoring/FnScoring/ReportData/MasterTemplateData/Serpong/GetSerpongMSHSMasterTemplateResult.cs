using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateData.Serpong
{
    public class GetSerpongMSHSMasterTemplateResult
    {
        public CreateHeaderBNSReportRequest Header { get; set; }
        public string HtmlOutput { get; set; }
        public List<string> GenerateStatus { get; set; }
        public string StorageSetting { get; set; }
        public GetSerpongMSHSMasterTemplateResult_Margin Margin { get; set; }
        public GetSerpongMSHSMasterTemplateResult_GlobalSetting GlobalSetting { get; set; }
    }
    public class GetSerpongMSHSMasterTemplateResult_Margin
    {
        //public Unit Unit { get; set; }
        public double? Top { get; set; }
        public double? Bottom { get; set; }
        public double? Left { get; set; }
        public double? Right { get; set; }
    }
    public class GetSerpongMSHSMasterTemplateResult_GlobalSetting
    {
        public int? Orientation { get; set; }
        public int? PaperSize { get; set; }
    }
}
