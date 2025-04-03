using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuum.LearningContinuumSummary
{
    public class GetListLearningContinuumSummaryRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string IdSubjectContinuum { get; set; }
        public string? IdStudent { get; set; }
    }
}
