using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction
{
    public class GetListGradeTeacherPrivilegeRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public bool IsAdvisor { get; set; }
    }
}
