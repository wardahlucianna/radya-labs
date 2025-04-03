using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.UpdateScore
{
    public class GetStudentScoreForApprovalResult
    {
        public string IdTransactionScore { get; set; }
        public string IdStudent { get; set; }
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
        public string IdScore { get; set; }
        //public decimal? OldRawScore { get; set; }
        //public decimal? OldScore { get; set; }
        public decimal? NewRawScore { get; set; }
        public decimal? NewScore { get; set; }
        public string TextScore { get; set; }
        public string TempTextScore { get; set; }
        public string Action { get; set; } //TransactionType
        public ApprovalStatus Status { get; set; }
        public string Comment { get; set; }
        public decimal SubComponentMaxScoreLength { set; get; }
    }
}
