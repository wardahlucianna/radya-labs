using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreComponent
{
    public class GetSubComponentCounterDetailForScoreComponentResult
    {
        public List<GetSubComponentCounterDetailForScoreComponentResult_Class> AvailableClassId { get; set; }
        public List<GetSubComponentCounterDetailForScoreComponentResult_Counter> Counters { get; set; }
    }

    public class GetSubComponentCounterDetailForScoreComponentResult_Class
    {
        public string Id { get; set; }
        public string Description { get; set; }
    }

    public class GetSubComponentCounterDetailForScoreComponentResult_Counter
    {
        public string IdCounter { get; set; }
        public string No { get; set; }
        public string Description { get; set; }
        public bool TeacherCanEditMaxRawScore { get; set; }
        public bool CanDelete { get; set; }
        public List<ItemValueVm> ClassIds { get; set; }
    }
}
