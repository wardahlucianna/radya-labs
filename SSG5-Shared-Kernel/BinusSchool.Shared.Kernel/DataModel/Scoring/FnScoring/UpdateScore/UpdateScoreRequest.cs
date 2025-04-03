using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.UpdateScore
{
    public class UpdateScoreRequest
    {
        public string IdSchool { set; get; }
        public string IdApprovalWorkflow { set; get; }
        public string TransactionType { set; get; }
        public string Comment { set; get; }
        public UpdateScoreStudent UpdateScoreStudent { set; get; }
    }
    public class UpdateScoreStudent
    {
        public string IdScore { set; get; } 
        public string IdStudent { set; get; }
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
        public string IdSubComponentCounter { set; get; }
        public string TextScore { set; get; }
        public decimal? RawScore { set; get; }
        public decimal MaxRawScore { set; get; }
        public string OldTextScore { set; get; }
        public decimal? OldRawScore { set; get; }
        public bool NeedApprovalUpdate { set; get; }
        public bool NeedCodeApprovalUpdate { set; get; }
        public ApprovalStatus Status { set; get; }
        public decimal SubComponentMaxScoreLength { set; get; }
    }
}
