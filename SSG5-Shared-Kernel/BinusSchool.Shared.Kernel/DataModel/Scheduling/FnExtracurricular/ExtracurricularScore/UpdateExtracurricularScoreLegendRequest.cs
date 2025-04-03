using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore
{
    public class UpdateExtracurricularScoreLegendRequest
    {
        public string IdSchool{ get; set; }
        public List<UpdateExtracurricularScoreLegendVm> ScoreLegends { get; set; }
    }

    public class UpdateExtracurricularScoreLegendVm
    {
        public string IdExtracurricularScoreLegend { get; set; }
        public string Score { get; set; }
        public string Description { get; set; }

    }
}
