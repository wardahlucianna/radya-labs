using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentAffectiveScore
{
    public class GetScoreViewStudentAffectiveScoreDetailRequest
    {
        public string IdAcademicYear { set; get; }
        public string IdGrade { set; get; }
        public string IdStudent { set; get; }
        public string IdComponent { set; get; }

    }
}
