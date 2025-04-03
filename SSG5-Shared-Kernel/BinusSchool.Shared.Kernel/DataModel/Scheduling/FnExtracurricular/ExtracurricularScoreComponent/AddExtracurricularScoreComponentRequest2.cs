using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScoreComponent
{
    public class AddExtracurricularScoreComponentRequest2
    {
        public string IdAcademicYear { get; set; }
        public List<ExtracurricularScoreComponentCategory> ScoreComponentCategories { get; set; }
        public List<string> ScoreCompCategoryDeleted { get; set; }


    }
    public class ExtracurricularScoreComponentCategory 
    {
        public string IdExtracurricularScoreCompCategory { get; set; }       
        public string Description { get; set; }
        public List<ExtracurricularScoreComponentVm> ScoreComponents { get; set; }
        public string IdExtracurricularScoreCalculationType { get; set; }
    }


}
