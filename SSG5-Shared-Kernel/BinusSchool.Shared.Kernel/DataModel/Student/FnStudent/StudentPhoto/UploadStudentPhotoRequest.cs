using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentPhoto
{
    public class UploadStudentPhotoRequest
    {
        public string IdStudentPhotoTransaction { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
}
