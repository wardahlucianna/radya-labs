using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByDept
{
    public class GetScoreSummaryByDeptResult
    {
        public ItemValueVm Department { get; set; }
        public int TotalCounter { get; set; }
        public int TotalSubmitted { get; set; }
        public int TotalPending { get; set; }
        public int TotalUnsubmitted { get; set; }
    }
}
