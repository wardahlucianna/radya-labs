using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.StudentParentScoreView
{
    public class GetAllScoreViewPeriodResult
    {
        public string IdPeriodViewScore { get; set; }
        public string AcademicYear { get; set; }
        public int Semester { get; set; }
        public string Term { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public DateTime? ClosingPeriodStart { get; set; }
        public DateTime? ClosingPeriodEnd { get; set; }
    }
}
