using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentStatus
{
    public class CheckStudentStatusHomeroomEnrollmentRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public int? Semester { get; set; }
        public string? IdPeriod { get; set; }
    }
}
