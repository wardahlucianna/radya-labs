using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore
{
    public class GetExtracurricularResult2
    {
        public string IdExtracurricular { get; set; }
        public string ExtracurricularName { get; set; }
        public DateTime? ScoreStartDate { get; set; }
        public DateTime? ScoreEndDate { get; set; }
        public bool InPeriodEntry { get; set; }
        public string Supervisor { get; set; }
        public string Coach { get; set; }
        public  List<ExtracurricularScoreLegendVm> ScoreLegends { get; set; }
    }


}
