using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateData
{
    public class CreateFooterBNSReportRequest
    {
        public string ContainerName { get; set; }
        public string IdReportTemplate { get; set; }
        public string TemplateName { get; set; }
        public string IdSchool { get; set; }
    }
}
