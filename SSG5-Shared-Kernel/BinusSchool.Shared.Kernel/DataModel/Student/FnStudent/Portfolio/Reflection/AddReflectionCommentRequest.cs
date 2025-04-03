using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Portfolio.Reflection
{
    public class AddReflectionCommentRequest
    {
        public string Comment { get; set; }
        public string IdUser { get; set; }
        public string IdReflection { get; set; }
    }
}
