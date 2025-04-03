using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData.Simprug
{
    public class GetSimprugDPProgressStatusRequest
    {
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public string IdPeriod { get; set; }
    }
}
