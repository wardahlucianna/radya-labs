using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreApproval
{
    public class GetDetailScoreApprovalRequest
    {
        public int? ApprovalType { set; get; }
        public string IdTransaction { set; get; }
    }
}
