using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreComponent
{
    public class GetScoreComponentListRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string IdSubject { get; set; }
    }
}
