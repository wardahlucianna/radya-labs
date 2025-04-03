using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherPrivilege
{
    public class GetAvailabilityPositionByIdUserRequest
    {
        public string IdAcademicyear { get; set; }
        public string IdUser { get; set; }
        public int Semester { get; set; }
    }
}
