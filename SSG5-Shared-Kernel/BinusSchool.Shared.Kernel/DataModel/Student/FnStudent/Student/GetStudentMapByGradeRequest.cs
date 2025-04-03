using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetStudentMapByGradeRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string AcademicYear { get; set; }
        public string Gender { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
    }
}
