using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.StudentParentScoreView
{
    public class GetScoreViewPeriodDetailResult
    {
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Term { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ItemValueVm Grade { get; set; }
    }
}
