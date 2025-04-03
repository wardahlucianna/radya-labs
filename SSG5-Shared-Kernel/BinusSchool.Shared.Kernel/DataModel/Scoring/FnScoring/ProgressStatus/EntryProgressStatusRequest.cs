using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ProgressStatus
{
    public class EntryProgressStatusRequest
    {
        public string PositionShortName { get; set; }
        public ApprovalStatus Action { get; set; }
        public List<EntryProgressStatusRequest_Body> ProgressStatusList { get; set; }
        public bool NeedApproval { get; set; }
        public string IdApprovalWorkflow { get; set; }
        public string IdUserActionNext { set; get; }
        public string UserIn { get; set; }
        public string IdGrade { get; set; }
    }
    public class EntryProgressStatusRequest_Body
    {
        public string IdStudent { get; set; }
        public ItemValueVm Homeroom { set; get; }
        public string IdUpdateStudentProgressStatus { get; set; }
        public EntryProgressStatusRequest_ProgressNationalMapping ProgressStatus { get; set; }
        public EntryProgressStatusRequest_ProgressNationalMapping OldProgressStatus { get; set; }
        public EntryProgressStatusRequest_ProgressNationalMapping NationalStatus { get; set; }
        public EntryProgressStatusRequest_ProgressNationalMapping OldNationalStatus { get; set; }
        public bool? ContinueAndSubmit { get; set; }
        public bool? OldContinueAndSubmit { get; set; }
        public bool? HideReportCard { get; set; }
        public bool? OldHideReportCard { get; set; }
        public string Remarks { get; set; }
        public string OldRemarks { get; set; }
        public string Comment { get; set; }
    }

    public class EntryProgressStatusRequest_ProgressNationalMapping
    {
        public int? IdProgressStudentStatusMapping { get; set; }
        public int? IdStudentStatus { get; set; }
    }
}
