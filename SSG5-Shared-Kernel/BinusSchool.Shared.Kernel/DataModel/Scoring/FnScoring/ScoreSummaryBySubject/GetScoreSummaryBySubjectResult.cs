using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryBySubject
{
    public class GetScoreSummaryBySubjectResult
    {
        public ItemValueVm ClassID { set; get; }
        public ItemValueVm Homeroom { set; get; }
        public ItemValueVm Subject { set; get; }
        public ItemValueVm SubjectType { set; get; }
        public int TotalCounter { set; get; }
        public int TotalSubmitted { set; get; }
        public int TotalPending { set; get; }
        public int TotalUnsubmitted { set; get; }
    }
}
