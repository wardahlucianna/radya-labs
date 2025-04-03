using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.StudentParentScoreView
{
    public class GetAllScoreViewPeriodRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }
        public int? Semester { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
    }
}
