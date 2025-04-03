using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportType
{
    public class UpdateReportTypeRequest
    {
        public string IdSchool { set; get; }
        public string? IdReportType { set; get; }
        public string Description { set; get; }
        public bool CurrentStatus { set; get; }
    }
}
