using System;
using System.Collections.Generic;
using System.Text;
using Org.BouncyCastle.Asn1.Crmf;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuumSettings.ExpectedLearningContinuumSettings
{
    public class GetExpectedLearningContinuumSettingsRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public int? Phase { get; set; }
        public string IdSubjectContinuum { get; set; }
        public string IdLearningContinuumType { get; set; } 
    }
}
