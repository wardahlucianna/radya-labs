using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectivesScoreGrade
{
    public class GetElectivesScoreGradeRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdExtracurricular { get; set; }
    }
}
