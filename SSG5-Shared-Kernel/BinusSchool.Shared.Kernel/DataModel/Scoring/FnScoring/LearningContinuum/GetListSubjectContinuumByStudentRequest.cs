using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuum
{
    public class GetListSubjectContinuumByStudentRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdGrade { get; set; }
        public string IdStudent { get; set; }
    }
}
