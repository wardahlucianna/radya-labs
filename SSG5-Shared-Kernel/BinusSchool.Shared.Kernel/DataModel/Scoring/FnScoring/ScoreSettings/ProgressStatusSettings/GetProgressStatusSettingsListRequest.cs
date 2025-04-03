using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ProgressStatusSettings
{
    public class GetProgressStatusSettingsListRequest : CollectionRequest
    {
        public string IdSchool { set; get; }
        public string IdAcademicYear { set; get; }
        public string? IdLevel { set; get; }
        public string? IdGrade { set; get; }
    }
}
