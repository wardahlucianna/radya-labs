using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentAcademicScore
{
    public class GetTotalCounterSummaryStudentScoreBySubjectTypeRequest
    {
        public string IdSchool { set; get; }
        public string IdAcademicYear { set; get; }
        public int Semester { set; get; }
        public string IdGrade { set; get; }
        public string IdStudent { set; get; }
        public string? IdSubjectType { set; get; }
    }
}
