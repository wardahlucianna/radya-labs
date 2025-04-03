using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectScoreLegend
{
    public class GetSubjectScoreLegendResult
    {
        public string IdSubjectScoreSetting { get; set; }
        public int? IdScoreLegend { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public string Grade { get; set; }
        public string Description { get; set; }
    }
}
