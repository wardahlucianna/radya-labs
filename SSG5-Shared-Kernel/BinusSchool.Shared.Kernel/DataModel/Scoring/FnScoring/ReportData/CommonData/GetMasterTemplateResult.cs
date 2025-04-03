using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.CommonData
{
    public class GetMasterTemplateResult_Margin
    {
        //public Unit Unit { get; set; }
        public double? Top { get; set; }
        public double? Bottom { get; set; }
        public double? Left { get; set; }
        public double? Right { get; set; }
    }
    public class GetMasterTemplateResult_GlobalSetting
    {
        public int? Orientation { get; set; }
        public int? PaperSize { get; set; }
    }
    public class GetMasterTemplateResult_File
    {
        public string ContainerName { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public Uri Location { get; set; }
        public string Html { get; set; }
    }
    public class GetMasterTemplateResult_OutputView
    {
        public string HtmlOutput { get; set; }
        public List<string> GenerateStatus { get; set; }
    }
}
