using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentGradingScoreGroupResult
    {
        public List<GetComponentGradingScoreGroupResult_SubjectList> SubjectList;
    }

    public class GetComponentGradingScoreGroupResult_SubjectList
    {
        public GetComponentGradingScoreGroupResult_header Header;

        public GetComponentGradingScoreGroupResult_body Body;
    }

    public class GetComponentGradingScoreGroupResult_header
    {
        public NameValueVm Subject;

        public List<NameValueVm> Period;
    }

    public class GetComponentGradingScoreGroupResult_body
    {
        public List<GetComponentGradingScoreGroupResult_component> ComponentList;
    }

    public class GetComponentGradingScoreGroupResult_component
    {
        public string Id;

        public string Name;

        public List<GetComponentGradingScoreGroupResult_Score> Score;
    }

    public class GetComponentGradingScoreGroupResult_Score
    {
        public string PeriodId;

        public string Desc;
    }
}
