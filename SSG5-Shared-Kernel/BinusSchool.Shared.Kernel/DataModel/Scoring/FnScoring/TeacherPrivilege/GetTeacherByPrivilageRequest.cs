using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherPrivilege
{
    public class GetTeacherByPrivilageRequest
    {
        public string IdPosition { get; set; }
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
    }
}
