using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnBlocking.StudentBlocking
{
    public class AddStudentBlockingRequest
    {
        public IEnumerable<StudentBlocking> StudentBlocking { get; set; }
    }

    public class StudentBlocking
    {
        public string IdStudent { get; set; }
        public string IdCategory { get; set; }
        public string IdType { get; set; }
        public bool IsBlock { get; set; }
    }
}
