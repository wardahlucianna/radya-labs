using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentNatcurCharacterValueResult
    {
        public List<GetComponentNatcurCharacterValueHandlerResult_ComponentList> Competencies;
    }

    public class GetComponentNatcurCharacterValueHandlerResult_ComponentList
    {
        public string Description;

        public string Score;
    }
}
