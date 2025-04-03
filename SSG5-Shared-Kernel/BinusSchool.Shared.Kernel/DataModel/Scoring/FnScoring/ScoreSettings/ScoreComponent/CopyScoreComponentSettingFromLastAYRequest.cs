using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreComponent
{
    public class CopyScoreComponentSettingFromLastAYRequest
    {
        public string IdUser { get; set; }
        public string CurrentIdAcademicYear { get; set; }
        public string PrevIdAcademicYear { get; set; }
    }
}
