using System;
using System.Collections.Generic;

namespace BinusSchool.Data.Model.School.FnSchool.Level
{
    public class GetLevelCodeRequest : CollectionSchoolRequest
    {
        public string CodeAcademicYear { get; set; }
        public string CodeLevel { get; set; }
    }
}
