using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.EntryScore
{
    public class GetComponentApprovalReportResult
    {
        public GetComponentApprovalReportResult_SubComponentCounterVm SubComponentCounterData { set; get; }
    }
    public class GetComponentApprovalReportResult_SubComponentCounterVm
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
        //public bool CanEntryScorebyPrivilege { set; get; }
        public bool CanEntryScoreInPeriod { set; get; }
        public DateTime? DateCounter { set; get; }
        //public int OrderNumberComponent { set; get; }
        //spublic int OrderNumberSubComponent { set; get; }

        public ItemValueVm CounterCategory { set; get; }
        public GetComponentApprovalReportResult_ComponentVm ComponentData { set; get; }
        public GetComponentApprovalReportResult_SubComponentVm SubComponentData { set; get; }

    }
    public class GetComponentApprovalReportResult_ComponentVm
    {
        public string IdComponent { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
        public bool AverageSubComponentScore { set; get; }
        public int OrderNumberComponent { set; get; }
    }

    public class GetComponentApprovalReportResult_SubComponentVm
    {
        public string IdSubComponent { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
        public decimal Weight { set; get; }
        public bool AverageCounterScore { set; get; }
        public decimal MaxScoreLength { set; get; }
        public int OrderNumberSubComponent { set; get; }
    }
}
