using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuum
{
    public class GetListLearningContinuumHistoryRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public string IdSubjectContinuum { get; set; }
    }
}
