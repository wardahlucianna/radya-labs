using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData.Serpong
{
    public class GetSerpongMSHSNatcurSubjectScoreRequest
    {
        public string ReportScoreTemplate { get; set; }
        public string ReportScoreTable { get; set; }
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public string IdPeriod { get; set; }
    }
}
