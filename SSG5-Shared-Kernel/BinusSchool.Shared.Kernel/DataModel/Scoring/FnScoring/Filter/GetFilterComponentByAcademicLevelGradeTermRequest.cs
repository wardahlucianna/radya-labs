using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.Filter
{
    public class GetFilterComponentByAcademicLevelGradeTermRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string PeriodCode { get; set; }
    }
}
