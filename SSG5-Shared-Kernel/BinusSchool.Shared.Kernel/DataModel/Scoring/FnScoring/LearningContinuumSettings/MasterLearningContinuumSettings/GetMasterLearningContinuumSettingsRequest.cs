using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuumSettings.MasterLearningContinuumSettings
{
    public class GetMasterLearningContinuumSettingsRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdSubjectContinuum { get; set; }  
        public string IdLearningContinuumType { get; set; }
        public string IdLOCCategory { get; set; }   
        public int Phase { get; set; }
    }
}
