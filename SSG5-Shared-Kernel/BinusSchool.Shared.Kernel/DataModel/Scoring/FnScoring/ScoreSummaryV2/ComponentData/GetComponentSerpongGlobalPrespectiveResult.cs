using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentSerpongGlobalPrespectiveResult
    {
        public ItemValueVm Period { get; set; }
        public int Semester { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<GetComponentGlobalPrespectiveResult_Component> Components { get; set; }
    }
        
    public class GetComponentGlobalPrespectiveResult_Component : ItemValueVm
    {
        public List<GetComponentGlobalPrespectiveResult_SubComponent> SubComponents { get; set; }
    }

    public class GetComponentGlobalPrespectiveResult_SubComponent : ItemValueVm
    {
        public string Score { get; set; }
    }
}
