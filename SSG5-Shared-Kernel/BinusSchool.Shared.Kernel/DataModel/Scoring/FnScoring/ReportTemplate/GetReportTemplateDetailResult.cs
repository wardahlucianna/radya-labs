using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportTemplate
{
    public class GetReportTemplateDetailResult
    {
        public string Code { get; set; }
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
        public int? Orientation { get; set; }
        public int? PageSize { get; set; }
        public double? MarginTop { get; set; }
        public double? MarginBottom { get; set; }
        public double? MarginLeft { get; set; }
        public double? MarginRight { get; set; }
        public string Template { get; set; }
        public bool CurrentStatus { get; set; }
        public int? StatusApproval { get; set; }
        public string IdSchool { get; set; }
        public string StyleCSS { get; set; }
        public string TemplateHeader { get; set; }
        public string TemplateSignature { get; set; }
    }
}
