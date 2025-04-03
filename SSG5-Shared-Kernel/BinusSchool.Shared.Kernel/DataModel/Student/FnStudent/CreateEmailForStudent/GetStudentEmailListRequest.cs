using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.CreateEmailForStudent
{
    public class GetStudentEmailListRequest : CollectionRequest
    {
        public string AcademicYearId { get; set; }
        public string SchoolId { get; set; }
        public string GradeId { get; set; }
        public string PathwayID { get; set; }
        public string StudentName { get; set; }
        public string StudentID { get; set; }
        public string Email { get; set; }
    }
}
