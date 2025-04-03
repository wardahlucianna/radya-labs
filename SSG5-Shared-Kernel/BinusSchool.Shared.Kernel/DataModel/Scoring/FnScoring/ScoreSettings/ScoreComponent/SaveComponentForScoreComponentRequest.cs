using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreComponent
{
    public class SaveComponentForScoreComponentRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string IdSubject { get; set; }
        public List<string> IdPeriod { get; set; }
        public string IdComponent { get; set; }
        public string ShortDesc { get; set; }
        public string Description { get; set; }
        public int OrderNo { get; set; }
        public bool AverageSubComponentScore { get; set; }
        public bool ShowGradingAsScore { get; set; }
        public int? IdScoreLegend { get; set; }
        public decimal? Weight { get; set; }
        public bool CalculateWeightFromSubComponent { get; set; }
        public bool IsLearningObjective { get; set; }
    }
}
