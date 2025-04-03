using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentScorePredictedGrade
{
    public class GetStudentScorePredictedGradeRequest
    {
        public string IdBinusian { set; get; }
        public string IdSchool { set; get; }
        public string IdAcademicYear { get; set; }
        public string IdGrade { set; get; }
        public int Semester { get; set; }
        public string IdPeriod { get; set; }
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdLesson { set; get; }
    }
}
