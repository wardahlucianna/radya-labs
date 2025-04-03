using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScoreComponent
{
    public class GetExtracurricularScoreComponentResult2 : CodeWithIdVm
    {       
        public List<ScoreComponentVm> ScoreComponentList { get; set; }
        public ItemValueVm CalculationType { get; set; }

    }
    public class ScoreComponentVm
    {
        public string IdExtracurricularScoreComponent { get; set; }
        public string Description { get; set; }
        public int OrderNumber { get; set; }
    }
}
