using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectScoreSetting
{
    public class GetSubjectScoreSettingResult
    {
        public string IdPeriod { get; set; }
        public string IdSubject { get; set; }
        //public string IdSubjectMappingSubjectLevel { get; set; }
        public string IdSubjectLevel { get; set; }
        public string IdApprovalWorkflow { get; set; }
        public bool ShowCodeOption { get; set; }
        public bool EnableTeacherJudgement { get; set; }
        public bool EnableDeleteAllScore { get; set; }
        public bool CanAddCounter { get; set; }
        public bool OneTimeInputScore { get; set; }
        public bool NeedChangeApproval { get; set; }
        public ApprovalStatus StatusApproval { get; set; }
    }
}
