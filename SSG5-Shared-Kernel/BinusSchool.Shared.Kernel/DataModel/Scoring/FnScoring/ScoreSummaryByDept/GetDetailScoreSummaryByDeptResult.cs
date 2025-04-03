using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByDept
{
    public class GetDetailScoreSummaryByDeptResult
    {
        public ItemValueVm Subject { get; set; }
        public ItemValueVm SubjectType { get; set; }
        public ItemValueVm Teacher { get; set; }
        public int TotalCounter { get; set; }
        public int TotalSubmitted { get; set; }
        public int TotalPending { get; set; }
        public int TotalUnsubmitted { get; set; }
    }
}
