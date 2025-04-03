using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreComponent
{
    public class SaveScoreComponentSettingRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string IdSubject { get; set; }
        public List<SaveScoreComponentSettingRequest_Setting> Settings { get; set; }
        public List<SaveScoreComponentSettingRequest_Term> Terms { get; set; }
    }

    public class SaveScoreComponentSettingRequest_Setting
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class SaveScoreComponentSettingRequest_Term
    {
        public string IdPeriod { get; set; }
        public int IdScoreLegend { get; set; }
        public decimal? Weight { get; set; }
    }
}
