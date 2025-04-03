using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryBySubject
{
    public class GetScoreSummaryStatisticHeaderBySubjectRequest
    {
        public string IdAcademicYear { set; get; }
        public string IdSchool { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
    }
}
