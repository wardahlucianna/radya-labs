using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateData
{
    public class CreateFooterBNSReportResult
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public Uri Location { get; set; }
        public string HtmlHeader { get; set; }
    }
}
