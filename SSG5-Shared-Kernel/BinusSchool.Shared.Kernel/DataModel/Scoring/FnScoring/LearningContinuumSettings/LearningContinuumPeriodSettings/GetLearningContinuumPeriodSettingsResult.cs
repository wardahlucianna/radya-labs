using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuumSettings.LearningContinuumPeriod
{
    public class GetLearningContinuumPeriodSettingsResult
    {
        public string IdLearningContinuumSetting { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
