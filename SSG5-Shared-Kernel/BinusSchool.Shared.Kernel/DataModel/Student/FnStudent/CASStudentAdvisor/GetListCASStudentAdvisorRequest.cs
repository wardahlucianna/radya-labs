using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.CASStudentAdvisor
{
    public class GetListCASStudentAdvisorRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string IdCASAdvisor { get; set; }
        public string HomeroomCode { get; set; }
    }
}
