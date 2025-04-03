using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreApproval
{
    public class SaveScoreApprovalActionV2Request
    {
        public string IdTransactionScoreHistory { get; set; }
        public ApprovalAction ApprovalAction { get; set; }
        public ApprovalTypeScoring ApprovalType { set; get; }
        public string Remarks { get; set; }
    }
}
