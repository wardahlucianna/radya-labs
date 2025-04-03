using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentPhoto
{
    public class CopyStudentPhotoRequest
    {
        public string IdAcademicYearFrom { get; set; }
        public string IdAcademicYearDest { get; set; }
        public List<string> IdStudents { get; set; }
    }
}
