using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.TeacherComment
{
    public class GetTeacherCommentDetailResult
    {
        public string IdTeacherCommentSetting { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Term { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MinCommentLength { get; set; }
        public int MaxCommentLength { get; set; }
        public int? MinSubjectCommentLength { get; set; }
        public int? MaxSubjectCommentLength { get; set; }
        public ItemValueVm WorkflowApproval { get; set; }
        public bool NeedChangeApproval { get; set; }
        public bool IsCoTeacherCanEdit { get; set; }
        public bool EnableSubjectTeacherComment { get; set; }
    }
}
