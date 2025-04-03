using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScoreComponent
{
    public class AddExtracurricularScoreComponentRequest
    {
        public string IdAcademicYear { get; set; }
        public List<ExtracurricularScoreComponentVm> ScoreComponents { get; set; }     
       
    }

    public class ExtracurricularScoreComponentVm
    {
        public int OrderNumber { get; set; }
        public string IdExtracurricularScoreComponent { get; set; }
        public string Description { get; set; }
    }
}
