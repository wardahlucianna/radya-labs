using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.CreateEmailForStudent
{
    public class GetStudentEmailListResult : CollectionRequest
    {
        public string StudentID { get; set; }
        public string StudentName { get; set; }
        public string Classroom { get; set; }
        public string Gender { get; set; }
        public string StudentEmailStatus { get; set; }
        public string StudentEmail { get; set; }
    }
}
