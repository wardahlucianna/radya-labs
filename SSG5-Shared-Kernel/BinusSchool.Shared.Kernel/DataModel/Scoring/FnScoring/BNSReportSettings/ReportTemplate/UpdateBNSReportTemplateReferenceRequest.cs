using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.BNSReportSettings.ReportTemplate
{
    public class UpdateBNSReportTemplateReferenceRequest
    {
        public string? IdReportTemplateReference { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Example { get; set; }
        public bool CurrentStatus { get; set; }
        public string IdSchool { get; set; }
        public bool IsDelete { get; set; }
    }
}
