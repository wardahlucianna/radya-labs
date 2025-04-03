using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentElectivesRequest
    {
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        //public string IdPeriod { get; set; }
        public int Semester { get; set; }

        public bool ShowElectivesByYear { get; set; }
        public bool ShowElectivesBySemester { get; set; }
        public bool ShowElectivesByCategory { get; set; }
        public bool ShowElectivesByList { get; set; }
    }
}
