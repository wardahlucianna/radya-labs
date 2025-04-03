using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuumSettings.ExpectedLearningContinuumSettings
{
    public class GetExpectedLearningContinuumSettingsResult
    {
        public string IdExpectedPhaseContinuum { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm SubjectContinuum { get; set; }
        public int Phase { get; set; }
        public ItemValueVm LearningContinuumType { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
