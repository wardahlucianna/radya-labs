using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreApproval
{
    public class GetListScoreApprovalResult
    {
        public GetListScoreApprovalWidget Widget { set; get; }
        public List<GetListScoreApprovalTable> ApprovalList { set; get; }
    }

    public class GetListScoreApprovalWidget
    {
        public int Approved { set; get; }
        public int Declined { set; get; }
        public int Waiting { set; get; }
    }

    public class GetListScoreApprovalTable
    {
        public string IdTransaction { set; get; }
        public string IdApprovalState { set; get; }
        public ApprovalTypeScoring ApprovalType { set; get; }
        public DateTime ActionDate { set; get; }
        public NameValueVm Student { set; get; }
        //public ItemValueVm Homeroom { set; get; }
        public ItemValueVm Subject { set; get; }
        public NameValueVm Requestor { set; get; }
        public string ApprovalStatus { set; get; }
        public string ApprovalStatusColor { set; get; }
        public ApprovalStatus IdApprovalStatus { set; get; }
        //public bool LastApproval { set; get; }
        public bool isAction { set; get; }
        public bool IsShow { set; get; }
        public string OldScore { set; get; }
        public string NewScore { set; get; }
    }

    public class GetListScoreApproval_FlowSuccess
    {
        public string IdApprovalWorkflow { set; get; }
        public string IdApprovalState { set; get; }
        public string IdToStateApprove { set; get; }
        public string StateName { set; get; }
        public ApprovalStateType StateType { set; get; }
        public int StateNumber { set; get; }
        public string IdTeacherPosition { set; get; }
        public string IdPosition { set; get; }
        public string CodePosition { set; get; }
    }

    public class GetListScoreApproval_ProgressStastusSetting
    {
        public string IdLevel { get; set; }
        public string LevelDesc { get; set; }
        public string IdGrade { get; set; }
        public string GradeDesc { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public bool NeedApproval { get; set; }
        public string IdApprovalWorkflow { get; set; }
    }
}
