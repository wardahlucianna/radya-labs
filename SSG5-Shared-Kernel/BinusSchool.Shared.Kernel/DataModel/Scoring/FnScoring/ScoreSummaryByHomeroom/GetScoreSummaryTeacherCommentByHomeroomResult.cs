using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByHomeroom
{
    public class GetScoreSummaryTeacherCommentByHomeroomResult
    {
        public List<ItemValueVm> Header { get; set; }
        public List<GetScoreSummaryTeacherCommentByHomeroomResult_Body> Body { get; set; }
    }
    public class GetScoreSummaryTeacherCommentByHomeroomResult_Body
    {
        public ItemValueVm Student { get; set; }
        public List<GetScoreSummaryTeacherCommentByHomeroomResult_Body_TeacherComment> TeacherComments { get; set; }
    }

    public class GetScoreSummaryTeacherCommentByHomeroomResult_Body_TeacherComment
    {
        public string IdPeriod { get; set; }
        public string Comment { get; set; }
    }

    public class GetScoreSummaryTeacherCommentByHomeroomResult_ComponentVm
    {
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string IdPeriod { get; set; }
        public string PeriodDescription { get; set; }
        public string? Comment { get; set; }
        public int Semester { get; set; }
        public string IdHomeroom { get; set; }
        public string Classroom { get; set; }
    }
}
