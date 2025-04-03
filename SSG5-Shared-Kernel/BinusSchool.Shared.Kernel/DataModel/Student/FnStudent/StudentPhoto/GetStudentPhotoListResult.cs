using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentPhoto
{
    public class GetStudentPhotoListResult
    {
        public string IdStudentPhoto { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public GetStudentPhotoResult_Student Student { get; set; }
        public ItemValueVm Grade { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }

    public class GetStudentPhotoResult_Student
    {
        public NameValueVm Student { get; set; }
        public string IdBinusian { get; set; }
    }
}
