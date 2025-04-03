using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.StudentParentScoreView
{
    public class SaveScoreViewPeriodRequest
    {
        public string IdScoreViewPeriod { get; set; }
        public string IdAcademicYear { get; set; }
        public string TermCode { get; set; }
        public List<string> Grades { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
