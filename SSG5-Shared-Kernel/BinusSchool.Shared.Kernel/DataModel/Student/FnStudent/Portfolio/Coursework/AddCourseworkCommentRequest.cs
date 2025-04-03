using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Portfolio.Coursework
{
    public class AddCourseworkCommentRequest
    {
        public string Comment { get; set; }
        public string IdUser { get; set; }
        public string IdCourseworkAnecdotalStudent { get; set; }
    }
}
