using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore
{
    public class GetExtracurricularScoreLegendResult2
    {
        public string IdExtracurricularScoreLegendCategory { get; set; }
        public string Description { get; set; }
        public List<ExtracurricularScoreLegendVm> ScoreLegends { get; set; }       
    }
    public class ExtracurricularScoreLegendVm
    {
        public string IdExtracurricularScoreLegend { get; set; }
        public string Score { get; set; }
        public string Description { get; set; }
    }
}
