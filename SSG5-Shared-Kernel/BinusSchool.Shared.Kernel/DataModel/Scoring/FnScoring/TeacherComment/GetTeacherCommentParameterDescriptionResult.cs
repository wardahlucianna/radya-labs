using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherComment
{
    public class GetTeacherCommentParameterDescriptionResult
    {
        public string School { set; get; }
        public string AcademicYear { set; get; }
        public int Semester { set; get; }
        public string Level { set; get; }
        public string Grade { set; get; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
    }
}
