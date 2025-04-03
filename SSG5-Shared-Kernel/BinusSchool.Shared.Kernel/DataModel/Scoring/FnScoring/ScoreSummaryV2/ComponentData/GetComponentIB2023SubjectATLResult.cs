using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentIB2023SubjectATLResult
    {
        public List<GetComponentIB2023SubjectATLResult_Header> Header { get; set; }
        public List<GetComponentIB2023SubjectATLResult_Body> Body { get; set; }
    }

    public class GetComponentIB2023SubjectATLResult_Header : NameValueVm
    {
        public List<string> ComponentList { get; set; }
    }
    public class GetComponentIB2023SubjectATLResult_Body
    {
        public NameValueVm Subject { get; set; }
        public string Teacher { get; set; }
        public List<GetComponentIB2023SubjectATLResult_ComponentScore> ComponentList { get; set; }
        public string TotalScore { get; set; }
        public string GradeScore { get; set; }
    }

    public class GetComponentIB2023SubjectATLResult_ComponentScore
    {
        public string ComponentLongDesc { get; set; }
        public string Score { set; get; }
    }
}
