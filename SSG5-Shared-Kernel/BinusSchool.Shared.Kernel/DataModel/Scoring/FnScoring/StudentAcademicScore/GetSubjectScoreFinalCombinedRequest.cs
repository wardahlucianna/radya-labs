using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentAcademicScore
{
    public class GetSubjectScoreFinalCombinedRequest
    {
        public string IdStudent { set; get; }
        public string IdAcademicYear { set; get; }
        public string IdGrade { set; get; }
        public string IdSubjectType { set; get; }
    }
}
