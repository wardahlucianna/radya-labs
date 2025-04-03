using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.PMBenchmark.PMBenchmarkEntry
{
    public class UpdatePMBenchmarkEntryRequest
    {
        public bool Status { set; get; }
        public List<UpdatePMBenchmarkEntryRequest_StudentScore> StudentScore { set; get; }
        public string Reason { set; get; }

    }

    public class UpdatePMBenchmarkEntryRequest_StudentScore
    {
        public string IdAssessmentScore { set; get; }
        public string IdUpdateTransaction { set; get; } //for submit draft
        public string IdAssessmentComponentSetting { set; get; }
        public string IdLesson { set; get; }
        public string IdStudent { set; get; }
        public int Score { set; get; }
    }
}
