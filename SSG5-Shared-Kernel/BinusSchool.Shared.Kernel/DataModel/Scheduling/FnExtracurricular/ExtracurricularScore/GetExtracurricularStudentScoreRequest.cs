using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore
{
    public class GetExtracurricularStudentScoreRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdExtracurricular { get; set; }
    }
}
