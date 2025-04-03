using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentGradingScoreResult
    {
        public List<GetComponentGradingScoreResult_Subject> SubjectList { get; set; }
    }
    public class GetComponentGradingScoreResult_Subject
    {
        public ItemValueVm Subject { get; set; }
        public List<GetComponentGradingScoreResult_Subject_Term> Terms { get; set; }
    }

    public class GetComponentGradingScoreResult_Subject_Term
    {
        public ItemValueVm Term { get; set; }
        public List<GetComponentGradingScoreResult_Subject_Term_Component> Components { get; set; }
    }

    public class GetComponentGradingScoreResult_Subject_Term_Component
    {
        public string ComponentDescription { get; set; }
        public List<GetComponentGradingScoreResult_Subject_Term_Competency>? ComponentScoreList { get; set; }
        public string ComponentScore { get; set; }
    }
    public class GetComponentGradingScoreResult_Subject_Term_Competency
    {
        public string DescriptionEnglish { get; set; }
        public string DescriptionIndonesia { get; set; }
        public string Score { get; set; }
    }
}
