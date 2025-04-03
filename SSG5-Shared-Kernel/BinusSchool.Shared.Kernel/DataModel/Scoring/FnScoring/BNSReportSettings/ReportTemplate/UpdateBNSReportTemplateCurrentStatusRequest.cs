using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.BNSReportSettings.ReportTemplate
{
    public class UpdateBNSReportTemplateCurrentStatusRequest
    {
        public string IdReportTemplate { get; set; }
        public bool CurrentStatus { get; set; }
    }
}
