using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreComponent
{
    public class SaveSubComponentForScoreComponentRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string IdSubject { get; set; }
        public string IdPeriod { get; set; }
        public List<string> IdComponent { get; set; }
        public string IdSubComponent { get; set; }
        public string ShortDesc { get; set; }
        public string Description { get; set; }
        public decimal? Weight { get; set; }
        public string IdScoreOption { get; set; }
        public int OrderNo { get; set; }
        public bool AverageCounterScore { get; set; }
    }
}
