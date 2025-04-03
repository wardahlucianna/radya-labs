using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherPrivilege
{
    public class GetAllDataTeacherPrivilegeRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string IdUser { get; set; }
        public int? Semester { get; set; }
        public string? IdStudent { get; set; }
    }
}
