using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuumSettings.MasterLearningContinuumSettings.MappingSubjectLearningContinuumSettings
{
    public class GetMappingSubjectLearningContinuumSettingsResult
    {
        public string IdSubjectContinuum { get; set; }
        public string SubjectName { get; set; }
        public string ShortName { get; set; }
        public List<ItemValueVm> MappedSubjects { get; set; }
        public bool IsCanDelete { get; set; }
    }
}
