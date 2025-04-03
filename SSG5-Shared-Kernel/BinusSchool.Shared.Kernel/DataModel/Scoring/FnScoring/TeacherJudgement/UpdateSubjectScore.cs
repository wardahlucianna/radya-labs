using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherJudgement
{
    public class UpdateSubjectScore
    {
        public string IdStudent { get; set; }
        public string IdSubjectScoreSetting { get; set; }
        public IEnumerable<SubjectScoreData> SubjectScoreVm { get; set; }
    }
    public class SubjectScoreData
    {
        public string IdComponent { get; set; }
        public decimal Score { get; set; }
        public bool IsAdjusmentScore { get; set; }
    }
}
