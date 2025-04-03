using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ProgressStatus
{
    public class GetStudentProgressStatusbyAcadYearRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }      
        public string IdGrade { get; set; }
        public string IdStudent { get; set; }
    }
}
