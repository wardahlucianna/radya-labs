using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction
{
    public class GetListStudentServiceAsActionRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public bool isAdvisor { get; set; } 
        public string IdGrade { get; set; }
        public string IdStatus { get; set; }
    }
}
