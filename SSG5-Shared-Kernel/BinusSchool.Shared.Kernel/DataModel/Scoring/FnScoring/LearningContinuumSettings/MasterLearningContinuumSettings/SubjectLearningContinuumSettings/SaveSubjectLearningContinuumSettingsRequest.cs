using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuumSettings.MasterLearningContinuumSettings.SubjectLearningContinuumSettings
{
    public class SaveSubjectLearningContinuumSettingsRequest
    {
        public string IdSubjectContinuum { get; set; }
        public string SubjectName { get; set; }
        public string ShortName { get; set; }   
        public List<string> IdSubjects { get; set; }
    }
}
