using System;
using System.Collections.Generic;

namespace BinusSchool.Data.Model.School.FnSchool.Grade
{
    public class GetGradeCodeRequest : CollectionSchoolRequest
    {
        public string CodeAcademicYear { get; set; }
        public string CodeLevel { get; set; }
    }
}
