using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentPhoto
{
    public class SaveStudentPhotoDataRequest
    {
        public string IdStudentPhoto { get; set; }
        public string IdStudent { get; set; }
        public string IdBinusian { get; set; }
        public string IdAcademicYear { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
}
