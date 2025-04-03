using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData
{
    public class GetSpiritReportRequest
    {
        public string ReportScoreTemplate { get; set; }
        public string ReportScoreTable { get; set; }
        public string TemplateName { get; set; }
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public string IdPeriod { get; set; }
    }

    public class GetSpiritReportRequest_ScoreGrading
    {
        public decimal Score { get; set; }
        public string Grading { get; set; }
    }
}
