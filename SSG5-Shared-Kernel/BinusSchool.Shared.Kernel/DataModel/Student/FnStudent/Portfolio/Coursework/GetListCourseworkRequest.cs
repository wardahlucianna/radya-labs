using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Portfolio.Coursework
{
    public class GetListCourseworkRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdStudent { get; set; }
        public int? Type { get; set; }
        public string IdUser { get; set; }
        public int? Semester { get; set; }
    }
}
