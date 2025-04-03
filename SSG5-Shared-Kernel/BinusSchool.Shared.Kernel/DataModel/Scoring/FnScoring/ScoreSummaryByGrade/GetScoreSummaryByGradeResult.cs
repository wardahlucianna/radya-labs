using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByGrade
{
    public class GetScoreSummaryByGradeResult
    {
        public ItemValueVm Grade { set; get; }
        public int OrderNumber { get; set; }
        public int TotalCounter { set; get; }
        public int TotalSubmitted { set; get; }
        public int TotalPending { set; get; }
        public int TotalUnsubmitted { set; get; }
    }
}
