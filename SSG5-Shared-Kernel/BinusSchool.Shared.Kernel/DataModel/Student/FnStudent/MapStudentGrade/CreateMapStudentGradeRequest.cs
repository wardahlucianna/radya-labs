using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MapStudentGrade
{
    public class CreateMapStudentGradeRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public IEnumerable<string> Ids { get; set; }
        public IEnumerable<string> RemoveIds { get; set; }
    }
}
