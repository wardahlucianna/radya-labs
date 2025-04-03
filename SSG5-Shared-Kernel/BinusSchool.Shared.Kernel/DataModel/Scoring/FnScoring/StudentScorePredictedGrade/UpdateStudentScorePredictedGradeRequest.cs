using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentScorePredictedGrade
{
    public class UpdateStudentScorePredictedGradeRequest
    {
        public string IdSubjectScoreSetting { get; set; }
        public string IdPeriod { get; set; }
        public List<StudentScorePredictedGradeVm> StudentScores { set; get; }
    }

    public class StudentScorePredictedGradeVm
    {
        public string IdStudent { get; set; }
        public string PredictedGrade { set; get; }
    }
}
