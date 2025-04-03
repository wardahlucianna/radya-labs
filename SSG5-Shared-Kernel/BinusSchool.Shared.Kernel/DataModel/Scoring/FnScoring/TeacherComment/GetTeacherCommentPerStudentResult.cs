using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherComment
{
    public class GetTeacherCommentPerStudentResult
    {
        public int MaxSemesterCount;
        public int MaxTermCount { get; set; }
        public List<GetTeacherCommentPerStudentResult_Semester> SemesterList { get; set; }
    }

    public class GetTeacherCommentPerStudentResult_Semester
    {
        public int Semester { get; set; }
        public List<GetTeacherCommentPerStudentResult_Period> PeriodCommentList { get; set; }
    }

    public class GetTeacherCommentPerStudentResult_Period
    {
        public string IdPeriod { get; set; }
        public string PeriodDescription { get; set; }
        public string Comment { get; set; }
    }
}
