using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuumSettings.MasterLearningContinuumSettings
{
    public class GetMasterLearningContinuumSettingsResult
    {
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm SubjectContinuum { get; set; }
        public int Phase { get; set; }
        public ItemValueVm LearningContinuumType { get; set; }
        public ItemValueVm LOCCategory { get; set; }
        public List<ItemValueVm> LOCItems { get; set; }
        public GetMasterLearningContinuumResult_LastUpdated LastUpdated { get; set; }
        public bool IsCanDelete { get; set; }
    }

    public class GetMasterLearningContinuumResult_LastUpdated
    {
        public DateTime? Date { get; set; }
        public string By { get; set; }
    }
}
