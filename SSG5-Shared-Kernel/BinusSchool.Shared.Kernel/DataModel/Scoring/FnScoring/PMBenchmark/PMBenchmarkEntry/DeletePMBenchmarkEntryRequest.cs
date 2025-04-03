using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.PMBenchmark.PMBenchmarkEntry
{
    public class DeletePMBenchmarkEntryRequest
    {
        public List<string> IdAssessmentScoreList {  get; set; }
        public string Reason { set; get; }
    }
}
