using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ProgressStatus
{
    public class GetProgressStatusResult
    {
        public DateTime? PeriodStartDate { get; set; }
        public DateTime? PeriodEndDate { get; set; }
        public bool? NeedApproval { get; set; }
        public string? IdApprovalWorkflow { get; set; }
        public GetProgressStatusResult_Option OptionUser { get; set; }
        public List<GetProgressStatusResult_Body> Body { get; set; }
    }
    public class GetProgressStatusResult_Option
    {
        public bool EnableProgressStatus { get; set; }
        public bool EnableContinueAndSubmit { get; set; }
        public bool EnableNationalStatus { get; set; }
        public bool EnableHideReportCard { get; set; }
    }

    public class GetProgressStatusResult_Body
    {
        public string IdUpdateStudentProgressStatus { get; set; }   // untuk draft
        public string IdLevel { get; set; }
        public string LevelDescription { get; set; }
        public string IdHomeroom { get; set; }
        public string HomeroomDescription { get; set; }
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public GetProgressStatusResult_Body_ProgressStatus ProgressStatus { get; set; }
        public GetProgressStatusResult_Body_NationalStatus NationalStatus { get; set; }
        public GetProgressStatusResult_Body_ContinueAndSubmit ContinueAndSubmit { get; set; }
        public GetProgressStatusResult_Body_HideReportCard HideReportCard { get; set; }
        public string Action { get; set; }      // approval / approvalUpdate / edit / waitingApproval
        public string Remarks { get; set; }
        public GetProgressStatusResult_Body_OptionPerStudent OptionPerStudent { get; set; }
    }

    public class GetProgressStatusResult_Body_ProgressStatus
    {
        public string? Id { get; set; }
        public int? IdStudentStatus { get; set; }
        public string Description { get; set; }
        public string OldDescription { get; set; }
        public bool? EnableAgreement { get; set; }
        public bool? EnableHideReportCard { get; set; }
    }
    public class GetProgressStatusResult_Body_NationalStatus
    {
        public string? Id { get; set; }
        public int? IdStudentStatus { get; set; }
        public string Description { get; set; }
        public string OldDescription { get; set; }
        public bool? EnableAgreement { get; set; }
        public bool? EnableHideReportCard { get; set; }
    }
    public class GetProgressStatusResult_Body_ContinueAndSubmit
    {
        public Nullable<bool> Value { get; set; }
        public Nullable<bool> OldValue { get; set; }
    }
    public class GetProgressStatusResult_Body_HideReportCard
    {
        public Nullable<bool> Value { get; set; }
        public Nullable<bool> OldValue { get; set; }
    }
    public class GetProgressStatusResult_Body_OptionPerStudent
    {
        public bool EnableProgressStatus { get; set; }
        public bool EnableContinueAndSubmit { get; set; }
        public bool EnableNationalStatus { get; set; }
        public bool EnableHideReportCard { get; set; }
    }

    public class GetProgressStatusResult_GetUserRole
    {
        public string IdUser { get; set; }
        public string IdRole { get; set; }
        public string RoleCode { get; set; }
        public string RoleGroupCode { get; set; }
    }

    public class GetProgressStatusResult_ProgressStastusSetting
    {
        public string IdLevel { get; set; }
        public string LevelDesc { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public bool NeedApproval { get; set; }
        public string IdApprovalWorkflow { get; set; }
    }
}
