using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.StudentParentScoreView
{
    public class GetAllGradeRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }
        public string TermCode { get; set; }
    }
}
