using System;
using System.Collections.Generic;

namespace BinusSchool.Data.Model.School.FnSchool.Level
{
    public class GetLevelRequest : CollectionSchoolRequest
    {
        public string IdAcadyear { get; set; }
        public string IdLevel { get; set; }
    }
}
