using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreComponent
{
    public class GetScoreComponentByPeriodRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string IdSubject { get; set; }
        public string IdPeriod { get; set; }
    }
}
