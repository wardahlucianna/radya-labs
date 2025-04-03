using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuumSettings.ExpectedLearningContinuumSettings
{
    public class GetExpectedLearningContinuumPhaseRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string IdSubjectContinuum { get; set; }
        public string IdLearningContinuumType { get; set; }
    }
}
