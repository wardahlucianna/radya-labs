using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreApproval
{
    public class GetHistoryScoreApprovalResult
    {
        public ApprovalStateType StateType { set; get; }
        public string TransactionType { set; get; }
        public string ApprovalAction { set; get; }
        public string ApprovalActionColor { set; get; }
        public ApprovalAction IdApprovalAction { set; get; }
        public string RequestorPosition { set; get; }
        public NameValueVm Requestor { set; get; }
        public string ApproverPosition { set; get; }
        public NameValueVm Approver { set; get; }
        public DateTime? ProcessDate { set; get; }
        public string Comment { set; get; }
    }
}
