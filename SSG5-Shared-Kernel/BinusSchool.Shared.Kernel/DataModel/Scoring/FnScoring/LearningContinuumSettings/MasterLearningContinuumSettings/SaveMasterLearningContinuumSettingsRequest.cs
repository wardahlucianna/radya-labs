using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuumSettings.MasterLearningContinuumSettings
{
    public class SaveMasterLearningContinuumSettingsRequest
    {
        public string IdAcademicYear { get; set; }  
        public string IdSubjectContinuum { get; set; }
        public string IdLearningContinuumType { get; set; }
        public string IdLOCCategory { get; set; }
        public int Phase { get; set; }
        public List<string> IdLOCItemList { get; set; }
        public bool IsUpdate { get; set; }
    }
}
