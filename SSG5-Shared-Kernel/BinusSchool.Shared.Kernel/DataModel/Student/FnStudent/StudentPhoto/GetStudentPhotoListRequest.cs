using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentPhoto
{
    public class GetStudentPhotoListRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public bool StudentType { get; set; }
        public string? IdLevel { get; set; }
        public string? IdGrade { get; set; }
    }
}
