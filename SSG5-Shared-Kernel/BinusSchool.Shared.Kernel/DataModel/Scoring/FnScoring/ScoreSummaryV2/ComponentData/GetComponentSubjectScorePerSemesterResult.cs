using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentSubjectScorePerSemesterResult
    {
        public List<GetComponentSubjectScorePerSemesterResult_Header> Header { get; set; }
        public List<GetComponentSubjectScorePerSemesterResult_Body> Body { get; set; }
    }

    public class GetComponentSubjectScorePerSemesterResult_Header : NameValueVm
    {
        public List<string> ComponentList { get; set; }
    }

    public class GetComponentSubjectScorePerSemesterResult_Body
    {
        public NameValueVm Subject { get; set; }
        public string Teacher { get; set; }
        public List<GetComponentSubjectScorePerSemesterResult_ComponentScore> ComponentList { get; set; }
        public string TotalScore { get; set; }
        public string GradeScore { get; set; }
    }

    public class GetComponentSubjectScorePerSemesterResult_ComponentScore
    {
        public string ComponentLongDesc { get; set; }
        public string Score { set; get; }
    }
}
