using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentScoreSummaryByLesson
{
    public class GetStudentScoreSubjectByLessonRequest
    {
        public string IdAcademicYear { set; get; }
        public int Semester { set; get; }
        public string IdGrade { set; get; }
        public string IdLesson { set; get; }
        public string IdSubjectType { set; get; }
        public string IdSubject { set; get; }  
        public decimal? MinScore { set; get; }
        public decimal? MaxScore { set; get; }

    }
}
