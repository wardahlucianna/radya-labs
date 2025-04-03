using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportTypeMapping
{
    public class GetReportTypeGradeMappedRequest
    {
        public string IdAcademicYear { set; get; }
        public string IdReportType { set; get; }
        public string IdReportTemplate { set; get; }
    }
}
