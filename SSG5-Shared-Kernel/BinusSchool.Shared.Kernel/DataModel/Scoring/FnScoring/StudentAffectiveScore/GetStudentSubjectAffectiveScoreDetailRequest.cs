using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentAffectiveScore
{
    public class GetStudentSubjectAffectiveScoreDetailRequest
    {
        public string IdAcademicYear { set; get; }
        public string IdGrade { set; get; }
        public string IdStudent { set; get; }
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdLesson { set; get; }
    }
}
