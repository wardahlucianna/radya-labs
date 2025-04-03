using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.PMBenchmark.PMBenchmarkEntry
{
    public class GetPMBenchmarkEntryRequest
    {
        public string IdAcademicYear { set; get; }
        public string IdGrade { set; get; }
        public string IdPeriod { set; get; }
        public string IdAssessmentType {set;get;}
        public string IdClass { set; get; }
    }
}
