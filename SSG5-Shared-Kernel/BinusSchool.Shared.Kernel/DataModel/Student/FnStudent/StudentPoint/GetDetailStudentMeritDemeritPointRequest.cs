using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentPoint
{
    public class GetDetailStudentMeritDemeritPointRequest
    {
        public string IdAcadyear { get; set; }
        public int? Semester { get; set; }
        public string IdStudent { get; set; }
    }
}
