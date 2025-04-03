using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentK13_SubjectScoreWithFormulaResult
    {
        public string SectionName { get; set; }
        public string SectionScore { get; set; }
        public List<GetComponentK13_SubjectScoreWithFormulaResult_Subjects> Subjects { get; set; }
    }
    public class GetComponentK13_SubjectScoreWithFormulaResult_Subjects
    {
        public string SubjectName { get; set; }
        public string SubjectTeacher { get; set; }
        public string?  SubjectScore { get; set; }
        public string? ScoreFormula { get; set; }
    }
}
