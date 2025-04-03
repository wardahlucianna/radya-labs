using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreApproval
{
    public class GetDetailScoreApprovalResult
    {
        public ApprovalTypeScoring ApprovalType { set; get; }
        public GetDetailScoreApprovalResult_UpdateScore Score { set; get; }
        public GetDetailScoreApprovalResult_ProgressStatus ProgressStatus { set; get; }
        public GetDetailScoreApprovalResult_TeacherComment TeacherComment { set; get; }
        public GetDetailScoreApprovalResult_SubjectMapping SubjectMapping { set; get; }
    }

    public class GetDetailScoreApprovalResult_UpdateScore
    {
        public string TransactionType { set; get; }
        public DateTime? RequestDate { set; get; }
        public string IdApprovalState { set; get; }
        public string ApprovalType { set; get; }
        public string AcademicYear { set; get; }
        public string Grade { set; get; }
        public NameValueVm Student { set; get; }
        public string Subject { set; get; }
        public string Component { set; get; }
        public string SubComponent { set; get; }
        public string SubComponentCounter { set; get; }
        public NameValueVm Requestor { set; get; }
        public string CurrentScore { set; get; }
        public string NewScore { set; get; }
        public string Reason { set; get; }
        public string ApprovalStatus { set; get; }
        public string ApprovalStatusColor { set; get; }
    }

    public class GetDetailScoreApprovalResult_ProgressStatus
    { }

    public class GetDetailScoreApprovalResult_TeacherComment
    { }

    public class GetDetailScoreApprovalResult_SubjectMapping
    { }
}
