using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore
{
    public class GetUnSubmittedScoreRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdBinusian { get; set; } // as supervisor or coach
    }
}
