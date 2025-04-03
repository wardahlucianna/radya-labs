using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore
{
    public class UpdateExtracurricularScoreLegendRequest2
    {
        public string IdSchool { get; set; }
        public List<ScoreLegendCategoryVm> ScoreLegendCategories { get; set; }
        public List<string> ScoreLegendCategoryDeleted { get; set; }
    }
    
    public class ScoreLegendCategoryVm
    {
        public string IdExtracurricularScoreLegendCategory { get; set; }
      
        public string Description { get; set; }
        public List<UpdateExtracurricularScoreLegendVm> ScoreLegends { get; set; }
    }
}
