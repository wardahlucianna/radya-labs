using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuumSettings.ExpectedLearningContinuumSettings
{
    public class SaveExpectedLearningContinuumSettingsRequest
    {
        public string IdExpectedPhaseContinuum { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdSubjectContinuum { get; set; }
        public int Phase { get; set; }
        public string IdLearningContinuumType { get; set; } 
    }
}
