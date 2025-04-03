using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{   
    public class GetAllStudentEnrollmentRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdLevel { get; set; }
    }
}
