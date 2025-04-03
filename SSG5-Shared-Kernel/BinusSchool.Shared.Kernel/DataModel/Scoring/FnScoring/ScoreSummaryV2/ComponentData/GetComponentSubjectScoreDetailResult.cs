using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentSubjectScoreDetailResult
    {
        public string Description { get; set; }
        public List<GetComponentSubjectScoreDetailResult_Component> ComponentList { get; set; }
    }
    public class GetComponentSubjectScoreDetailResult_Component
    {
        public string IdComponent { get; set; }
        public string ComponentName { get; set; }
        public List<string> Counters { get; set; }
        public List<GetComponentSubjectScoreDetailResult_Component_SubComponent> SubComponentList { get; set; }
        public string TotalWeight { get; set; }
        public string TotalScore { get; set; }
        public string Overall { get; set; }
    }
    
    public class GetComponentSubjectScoreDetailResult_Component_SubComponent 
    { 
        public string IdSubComponent { get; set; }
        public string SubComponentName { get; set; }
        public string SubComponentScore { get; set; }
        public string SubComponentWeighted { get; set; }
        public List<GetComponentSubjectScoreDetailResult_Component_SubComponent_ComponentCounter> ComponentCounterList { get; set; }
    }

    public class GetComponentSubjectScoreDetailResult_Component_SubComponent_ComponentCounter
    {
        public string IdSubComponentCounter { get; set; }
        public string Counter { get; set; }
        public string CountersScore { get; set; }
    }
}
