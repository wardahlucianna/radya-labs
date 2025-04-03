using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.BNSReportSettings.ReportTemplate
{
    public class SaveBNSReportTemplateRequest
    {
        public string IdReportTemplate { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public int Orientation { get; set; }
        public double MarginTop { get; set; }
        public double MarginBottom { get; set; }
        public double MarginLeft { get; set; }
        public double MarginRight { get; set; }
        public bool CurrentStatus { get; set; }
        public string Template { get; set; }
        public int ApprovalStatus { get; set; }
        public string IdSchool { get; set; }
        public string Code { get; set; }
        public int PageSize { get; set; }
        public string TemplateHeader { get; set; }
        public string TemplateFooter { get; set; }
        public string TemplateSignature { get; set; }
        public string StyleCSS { get; set; }
        public string? Title { get; set; }
        public string? TOCGroup { get; set; }
        public int? PageNumber { get; set; }
        public string? Categories { get; set; }
    }
}
