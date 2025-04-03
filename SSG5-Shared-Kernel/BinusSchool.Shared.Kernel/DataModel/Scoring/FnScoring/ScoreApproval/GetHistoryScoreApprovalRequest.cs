using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreApproval
{
    public class GetHistoryScoreApprovalRequest
    {
        public int? ApprovalType { set; get; }
        public string IdTransaction { set; get; }
    }
}
