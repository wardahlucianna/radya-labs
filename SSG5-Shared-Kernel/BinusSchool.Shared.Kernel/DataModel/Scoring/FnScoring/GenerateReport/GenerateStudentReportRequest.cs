using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.GenerateReport
{
    public class GenerateStudentReportRequest
    {
        public string IdReportType { get; set; }
        public string IdPeriod { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string EmailRequester { get; set; }
    }
}
