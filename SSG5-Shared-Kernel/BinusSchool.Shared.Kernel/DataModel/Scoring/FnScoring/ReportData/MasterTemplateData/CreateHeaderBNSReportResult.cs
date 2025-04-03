using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateData
{
    public class CreateHeaderBNSReportResult
    {
        public CreateHeaderBNSReportDetail Header { get; set; }
        public CreateHeaderBNSReportDetail Footer { get; set; }
    }

    public class CreateHeaderBNSReportDetail
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public Uri Location { get; set; }
        public string Html { get; set; }
        public double? Spacing { get; set; }
        public string ContainerName { get; set; }
    }
}
