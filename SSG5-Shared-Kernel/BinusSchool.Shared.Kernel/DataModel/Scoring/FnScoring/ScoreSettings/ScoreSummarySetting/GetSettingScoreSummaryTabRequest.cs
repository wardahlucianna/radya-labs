using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreSummarySetting
{
    public class GetSettingScoreSummaryTabRequest : CollectionSchoolRequest
    {
        //public string IdUser { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
    }
}
