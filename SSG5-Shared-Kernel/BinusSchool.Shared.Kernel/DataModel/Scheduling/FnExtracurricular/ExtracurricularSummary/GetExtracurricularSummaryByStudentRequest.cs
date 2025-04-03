using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularSummary
{
    public class GetExtracurricularSummaryByStudentRequest
    {
        //public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public string IdStudent { get; set; }
    }
}
