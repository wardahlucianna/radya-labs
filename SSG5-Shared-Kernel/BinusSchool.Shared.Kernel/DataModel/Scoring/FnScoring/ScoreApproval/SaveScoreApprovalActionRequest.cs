using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreApproval
{
    public class SaveScoreApprovalActionRequest
    {
        public string IdStudent { set; get; }
        public string Comment { set; get; }
        public string IdTransaction { set; get; }
        public string IdApprovalState { set; get; }
        public ApprovalAction ApprovalAction { set; get; }
        public ApprovalTypeScoring ApprovalType { set; get; }
    }
}
