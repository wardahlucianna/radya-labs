using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ProgressStatusSettings
{
    public class GetProgressStatusSettingsDetailResult
    {
        public string IdProgressStatusSetting { set; get; }
        public CodeWithIdVm AcademicYear { set; get; }
        public CodeWithIdVm Grade { set; get; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public bool NeedApproval { set; get; }
        public List<GetProgressStatusSettingsDetailResult_ProgressStudentStatus> ProgressStudentStatus { set; get; }
    }

    public class GetProgressStatusSettingsDetailResult_ProgressStudentStatus
    {
        public string IdProgressStudentStatusMapping { set; get; }
        public GetProgressStatusSettingsDetailResult_StudentStatus StudentStatus { set; get; }
        public string StudentProgressText { set; get; }
        public string StudentProgressBahasaText { set; get; }
        public bool EnableAgreement { set; get; }
        public bool EnableHideReportCard { set; get; }
    }

    public class GetProgressStatusSettingsDetailResult_StudentStatus
    {
        public int IdStudentStatus { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
    }
}
