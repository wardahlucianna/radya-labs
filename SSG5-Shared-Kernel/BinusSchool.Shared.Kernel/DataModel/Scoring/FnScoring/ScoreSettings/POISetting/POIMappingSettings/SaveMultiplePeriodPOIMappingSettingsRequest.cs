using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.POISetting.POIMappingSettings
{
    public class SaveMultiplePeriodPOIMappingSettingsRequest
    {
        public List<string> IdProgrammeInqs { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
