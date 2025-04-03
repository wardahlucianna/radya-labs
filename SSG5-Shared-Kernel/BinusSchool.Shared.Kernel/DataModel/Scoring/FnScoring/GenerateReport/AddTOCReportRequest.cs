using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.GenerateReport
{
    public class AddTOCReportRequest
    {
        public string IdHomeroom { get; set; }
        public string IdStudent { get; set; }
        public string IdReport { get; set; }
        public string ReportTitle { get; set; }
        public int? PageNumber { get; set; }
        public string TOCGroup { get; set; }
        public bool IsDelete { get; set; }
    }
}
