using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData
{
    public class GetElectivesReportRequest
    {
        public string TemplateName { get; set; }
        public string ReportScoreTemplate { get; set; }
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public string IdPeriod { get; set; }
        public string Code { get; set; }
    }

    public class GetElectivesReportRequest_Legend
    {
        public string Predikat { get; set; }
        public decimal MinScore { get; set; }
        public decimal MaxScore { get; set; }
        public string Description { get; set; }
    }
}
