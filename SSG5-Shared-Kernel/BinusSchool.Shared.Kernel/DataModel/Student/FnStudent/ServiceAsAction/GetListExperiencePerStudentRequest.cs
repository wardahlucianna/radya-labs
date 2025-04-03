using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction
{
    public class GetListExperiencePerStudentRequest
    {
        public string IdUser { get; set; }
        public string IdStudent { get; set; }
        public string IdAcademicYear { get; set; }
        public bool IsSupervisor { get; set; }
    }
}
