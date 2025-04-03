using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuum.LearningContinuumEntry
{
    public class GetListStudentLearningContinuumEntryRequest
    {
        public string IdSchool { set; get; }
        public string IdUser { get; set; }
        public string IdAcademicYear { set; get; }
        public int Semester { get; set; }
        public string? IdClass { set; get; }
        public string? IdHomeroom { get; set; }
        public string? IdStudent { get; set; }

    }
}
