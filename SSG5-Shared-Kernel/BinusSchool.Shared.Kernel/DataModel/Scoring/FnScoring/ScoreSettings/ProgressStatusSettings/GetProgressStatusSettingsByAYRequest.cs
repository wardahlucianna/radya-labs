using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ProgressStatusSettings
{
    public class GetProgressStatusSettingsByAYRequest
    {
        public string ActiveIdAcademicYear { set; get; }
        public string CopyIdAcademicYear { set; get; }
        public string ActiveGradeCode { set; get; }
    }
}
