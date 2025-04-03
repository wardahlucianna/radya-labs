using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherJudgement
{
    public class UpdateComponentScore
    {
        public string IdStudent { get; set; }
        public string IdComponent { get; set; }
        public IEnumerable<CompScoreData> ComponentScoreVm { get; set; }
    }

    public class CompScoreData
    {
        public string IdSubComponent { get; set; }
        public decimal Score { get; set; }
        public bool IsAdjusmentScore { get; set; }
    }
}
