using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ProgressStatusSettings
{
    public class GetProgressStatusSettingsListResult : ItemValueVm
    {
        public string IdProgressStatusSetting { set; get; }
        public CodeWithIdVm AcademicYear { set; get; }
        public CodeWithIdVm Level { set; get; }
        public CodeWithIdVm Grade { set; get; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public bool NeedApproval { get; set; }
        public string IdApprovalWorkflow { get; set; }
        public List<GetProgressStatusSettingsResult_PIC> PIC { set; get; }
    }

    public class GetProgressStatusSettingsResult_PIC
    {
        public string IdProgressStatusPIC { set; get; }
        public string IdLevel { set; get; }
        public CodeWithIdVm Role { set; get; }
        public CodeWithIdVm TeacherPosition { set; get; }
        public bool EnableProgressStatus { set; get; }
        public bool EnableNationalStatus { set; get; }
        public bool EnableAgreement { set; get; }
        public bool EnableHideReportCard { set; get; }
    }

    public class GetProgressStatusSettingsResult_ProgressStastusSetting
    {
        public string IdProgressStatusSetting { get; set; }
        public string IdLevel { get; set; }
        public string LevelDesc { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public bool NeedApproval { get; set; }
        public string IdApprovalWorkflow { get; set; }
    }
}
