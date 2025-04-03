using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentIB2023NationalRequirementsResult
    {
        public List<NameValueVm> Header { get; set; }
        public List<GetComponentIB2023NationalRequirementsResult_Body> Body { get; set; }
        public string TotalScore { get; set; }
    }

    public class GetComponentIB2023NationalRequirementsResult_Body
    {
        public NameValueVm Subject { get; set; }
        public string Teacher { get; set; }
        public string? Score { get; set; }
        public string? CompetenciesOutcomes { get; set; }
    }
}
