using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.BNSReportSettings.ReportTemplate
{
    public class UpdateMasterTOCReportRequest
    {
        public string IdLevel { get; set; }
        public string IdReportType { get; set; }
        public string Title { get; set; }
        public string TOCGroup { get; set; }
        public bool IsDelete { get; set; }
    }
}
