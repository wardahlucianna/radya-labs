using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Portfolio.Reflection
{
    public class GetReflectionRequest : CollectionSchoolRequest
    {
        public string IdStudent { get; set; }
        public string IdUser { get; set; }
        public string IdAcademicYear { get; set; }
        public int? Semester { get; set; }
    }
}
