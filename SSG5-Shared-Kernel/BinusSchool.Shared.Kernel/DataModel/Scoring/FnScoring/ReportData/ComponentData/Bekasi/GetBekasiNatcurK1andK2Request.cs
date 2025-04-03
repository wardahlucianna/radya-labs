using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData.Bekasi
{
    public class GetBekasiNatcurK1andK2Request
    {
        public string ReportScoreTemplate { get; set; }
        public string TemplateName { get; set; }
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public string IdPeriod { get; set; }
    }
}
