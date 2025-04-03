using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectScoreLegend
{
    public class GetSubjectScoreLegendRequest
    {
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdPeriod { set; get; }
    }
}
