using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.EntryScore
{
    public class GetSettingEntryScoreResult
    {        
        public bool EnableTeacherJudgement { get; set; }
        public bool EnableDeleteAllScore { get; set; }
        public bool OnTimeInputScore { get; set; }
        public bool NeedEntryApproval { get; set; }
        public bool NeedEntryCodeApproval { get; set; }
        public bool NeedChangeApproval { get; set; }
        public bool NeedChangeCodeApproval { get; set; }
        public bool NeedDeleteApproval { get; set; }
        public DateTime? EntryScoreStartDate { get; set; }
        public DateTime? EntryScoreEndDate { get; set; }
        public bool ShowCodeOption { get; set; }
        public List<ScoreAdditionalCodeVm> ScoreAdditionalCodeList { set; get; }
        public bool CanAddCounter { get; set; }
        public string IdApprovalWorkflow { get; set; }
        public bool isProrateScore { get; set; }
        public bool EnablePredictedGrade { get; set; }
        public List<SubjectScoreLegendVm> SubjectScoreLegendList { get; set; }
        public DateTime? UpdateScoreStartDate { get; set; }
        public DateTime? UpdateScoreEndDate { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsFinalApprover { get; set; }
        public bool? IsEditable { get; set; }
        public bool? IsLocked { get; set; }
    }

    public class ScoreAdditionalCodeVm
    {
        public decimal Key { set; get; }
        //public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
        public bool CurrentStatus { set; get; }

    }

  
}
