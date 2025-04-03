using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.CASStudentAdvisor
{
    public class SaveCasStudentMappingAdvisorRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }

        public List<SaveCasStudentMappingAdvisorRequest_Data> Data { get; set; }
    }

    public class SaveCasStudentMappingAdvisorRequest_Data
    {
        public string IdCasAdvisorStudent { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string IdCasAdvisor { get; set; }
    }
}
