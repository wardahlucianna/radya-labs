using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentMYP2023ProgressStatusResult
    {
        public List<GetComponentMYP2023ProgressStatusResult_DataValue> DataList { get; set; }
    }

    public class GetComponentMYP2023ProgressStatusResult_DataValue
    {
        public List<string> Value { get; set; }
    }
}
