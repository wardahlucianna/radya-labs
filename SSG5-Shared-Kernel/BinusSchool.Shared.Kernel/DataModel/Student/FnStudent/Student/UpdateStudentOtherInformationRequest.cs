using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class UpdateStudentOtherInformationRequest
    {
        public string IdStudent { get; set; }
        public string FutureDream { get; set; }
        public string Hobby { get; set; }
        public int IsParentUpdate { get; set; }
    }
}
