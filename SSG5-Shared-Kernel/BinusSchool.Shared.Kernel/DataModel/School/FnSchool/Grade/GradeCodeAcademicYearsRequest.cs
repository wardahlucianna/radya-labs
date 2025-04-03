using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.Grade
{
    public class GradeCodeAcademicYearsRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string CodeAcademicYearStart { get; set; }
        public string CodeAcademicYearEnd { get; set; }
    }
}
