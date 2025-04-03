using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportTypeMapping
{
    public class UpdateReportTypeMappingRequest
    {
        public string IdReportTypeMapping { set; get; }
        public string IdReportType { set; get; }
        public string IdReportTemplate { set; get; }
        public List<string> GradeList { set; get; }
        public bool Status { set; get; }
    }
}
