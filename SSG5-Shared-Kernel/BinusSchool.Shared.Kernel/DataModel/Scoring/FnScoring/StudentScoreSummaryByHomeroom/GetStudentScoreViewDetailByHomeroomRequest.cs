using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentScoreSummaryByHomeroom
{
    public class GetStudentScoreViewDetailByHomeroomRequest
    {
        public string IdAcademicYear { set; get; }
        public int Semester { set; get; }
        public string IdSubject{ set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdStudent { set; get; }


    }
}
