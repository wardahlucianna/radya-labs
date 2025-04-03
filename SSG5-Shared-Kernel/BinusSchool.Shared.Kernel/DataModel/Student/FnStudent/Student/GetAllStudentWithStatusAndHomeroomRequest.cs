using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetAllStudentWithStatusAndHomeroomRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
    }
}
