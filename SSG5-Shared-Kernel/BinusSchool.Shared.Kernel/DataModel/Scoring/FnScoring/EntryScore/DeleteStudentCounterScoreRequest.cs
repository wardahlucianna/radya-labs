using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.EntryScore
{
    public class DeleteStudentCounterScoreRequest
    {
        public string IdApprovalWorkflow { get; set; }
        public List<DeleteStudentCounterScoreVm> StudentScores { set; get; }
    }

    public class DeleteStudentCounterScoreVm
    {
        public string IdStudent { set; get; }
        public string IdSubComponentCounter { set; get; }
        public string IdTransactionScore { set; get; }
    }
}
