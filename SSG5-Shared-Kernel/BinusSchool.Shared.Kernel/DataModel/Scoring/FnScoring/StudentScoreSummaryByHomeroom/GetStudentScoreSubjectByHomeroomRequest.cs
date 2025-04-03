using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentScoreSummaryByHomeroom
{
    public class GetStudentScoreSubjectByHomeroomRequest
    {
        public string IdSchool { set; get; }
        public string IdAcademicYear { set; get; }
        public int Semester { set; get; }
        public string IdGrade { set; get; }
        public string IdHomeroom { set; get; }
        public string IdSubjectType { set; get; }
        public string IdSubject { set; get; }
        //public string IdComponent { set; get; }
        //public string IdSubComponent { set; get; }
        //public string IdSubComponentCounter { set; get; }
        public decimal? MinScore { set; get; }
        public decimal? MaxScore { set; get; }
    }
}
