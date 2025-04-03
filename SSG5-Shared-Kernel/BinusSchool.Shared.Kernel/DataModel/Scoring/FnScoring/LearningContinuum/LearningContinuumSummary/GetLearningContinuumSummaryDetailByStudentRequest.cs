using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuum.LearningContinuumSummary
{
    public class GetLearningContinuumSummaryDetailByStudentRequest
    {
        public string IdFilterAcademicYear { get; set; }
        public int FilterSemester { get; set; }
        public string IdFilterLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdFilterHomeroom { get; set; }
        public string IdFilterGrade { get; set; }
        public string IdFilterSubjectContinuum { get; set; }
        public string IdSubjectContinuum { get; set; }
        public string IdStudent { get; set; }
    }
}
