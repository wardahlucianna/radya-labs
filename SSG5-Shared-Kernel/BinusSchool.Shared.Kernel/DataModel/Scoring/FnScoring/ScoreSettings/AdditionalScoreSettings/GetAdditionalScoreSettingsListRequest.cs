using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.AdditionalScoreSettings
{
    public class GetAdditionalScoreSettingsListRequest : CollectionRequest
    {
        public string IdAcademicYear { set; get; }
        public string? IdLevel { set; get; }
        public string? IdGrade { set; get; }
    }
}
