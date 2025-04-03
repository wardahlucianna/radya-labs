using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuum
{
    public class GetLearningContinuumRequest
    {
        public string IdSchool { get; set; }
        public string IdUser { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
    }
}
