using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.TeacherComment
{
    public class SaveTeacherCommentRequest
    {
        public string IdTeacherCommentSetting { get; set; }
        public string IdSchool { get; set; }
        public string TermCode { get; set; }
        public List<string> Grades { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MinCommentLength { get; set; }
        public int MaxCommentLength { get; set; }
        public int? MinSubjectCommentLength { get; set; }
        public int? MaxSubjectCommentLength { get; set; }
        public string IdApprovalWorkflow { get; set; }
        //public bool NeedChangeApproval { get; set; } //Temporary Unsed Field
        public bool IsCoTeacherCanEdit { get; set; }
        public bool EnableSubjectTeacherComment { get; set; }
    }
}
