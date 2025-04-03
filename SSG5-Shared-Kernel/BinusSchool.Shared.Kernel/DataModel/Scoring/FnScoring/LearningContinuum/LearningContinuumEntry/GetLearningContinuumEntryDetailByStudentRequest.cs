using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuum.LearningContinuumEntry
{
    public class GetLearningContinuumEntryDetailByStudentRequest
    {
        public string IdUser { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string IdStudent { get; set; }
        public string IdSubjectContinuum { get; set; }
        public int Semester { get; set; }
        public string IdSchool { get; set; }
        public string? IdClass { get; set; }
        public string? IdHomeroom { get; set; }
    }
}
