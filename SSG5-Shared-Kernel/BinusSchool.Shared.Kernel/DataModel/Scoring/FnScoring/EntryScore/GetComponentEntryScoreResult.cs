using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.EntryScore
{
    public class GetComponentEntryScoreResult
    {
        public SubComponentCounterVm SubComponentCounterData { set; get; }
    }
    public class SubComponentCounterVm
    {
        public string IdSubComponentCounter { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
        public decimal Weight { set; get; }
        public decimal MaxRawScore { set; get; }
        public bool CanUpdateMaxRawScore { set; get; }
        public string IdTeacherPosition { set; get; }
        public DateTime? StartDate { set; get; }
        public DateTime? EndDate { set; get; }
        public bool CanEntryScorebyPrivilege { set; get; }
        public bool CanEntryScoreInPeriod { set; get; }
        public DateTime? DateCounter { set; get; }

        public ItemValueVm CounterCategory { set; get; }
        public ComponentVm ComponentData { set; get; }
        public SubComponentVm SubComponentData { set; get; }

    }
    public class ComponentVm
    {
        public string IdComponent { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
        public bool AverageSubComponentScore { set; get; }
    }

    public class SubComponentVm
    {
        public string IdSubComponent { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
        public decimal Weight { set; get; }
        public bool AverageCounterScore { set; get; }
        public decimal MaxScoreLength { set; get; }


    }



}
