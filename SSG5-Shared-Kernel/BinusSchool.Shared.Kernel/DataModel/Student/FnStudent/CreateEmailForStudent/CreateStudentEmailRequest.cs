using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.CreateEmailForStudent
{
    public class CreateStudentEmailRequest
    {
        public string StudentId { get; set; }
        public string Email { get; set; }
        public string UserIn { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
    }
}
