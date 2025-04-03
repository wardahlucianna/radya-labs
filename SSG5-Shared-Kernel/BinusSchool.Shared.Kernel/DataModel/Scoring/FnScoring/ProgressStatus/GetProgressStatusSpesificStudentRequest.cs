using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ProgressStatus
{
    public class GetProgressStatusSpesificStudentRequest
    {
        //public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string IdStudent { get; set; }
    }
}
