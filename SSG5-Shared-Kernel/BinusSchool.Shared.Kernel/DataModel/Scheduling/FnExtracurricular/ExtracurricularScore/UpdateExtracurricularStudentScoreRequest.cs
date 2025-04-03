using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore
{
    public class UpdateExtracurricularStudentScoreRequest
    {
        public string IdExtracurricular { get; set; }
        public List<UpdateExtracurricularStudentScoreVm> StudentScores { get; set; }
    }

    public class UpdateExtracurricularStudentScoreVm
    {       
        public string IdExtracurricularScoreEntry { get; set; }
        public string IdStudent { get; set; }
        public string IdExtracurricularScoreComponent { get; set; }        
        public string IdExtracurricularScoreLegend { get; set; }
    }
}
