using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Scoring.FnScoring.TeacherPrivilege
{
    public class GetAllDataTeacherAssignmentPrivilegeRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string IdUser { get; set; }
        public int? Semester { get; set; }
        public string? IdStudent { get; set; }
    }
}
