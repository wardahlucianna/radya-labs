using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetStudentByIdParentRequest
    {      
        public string IdParent { get; set; }
        public List<int> IdStudentStatusExcludeList { get; set; }
        public string Search { get; set; }
    }
}
