using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.TeacherComment
{
    public class GetAllTeacherCommentResult
    {
        public string IdCommentSetting { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public int Semester { get; set; }
        public ItemValueVm Term { get; set; }
        public int MinCommentLength { get; set; }
        public int MaxCommentLenght { get; set; }
        public int? MinSubjectCommentLength { get; set; }
        public int? MaxSubjectCommentLength { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool EnableSubjectTeacherComment { get; set; }
        public bool IsCanDelete { get; set; }
    }
}
