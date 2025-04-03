using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentEnrollmentScore
{
    public class GetStudentSubjectScoreDetailIBRequest
    {
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public int Semester { set; get; }
        public string IdGrade { set; get; }
        public string IdStudent { set; get; }
    }
}
