using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.MeritDemerit
{
    public class AddMeritDemeritSanctionMappingCopyRequest
    {
        public string IdAcademicYearCopyTo { get; set; }
        public List<string> IdSunctionMapping { get; set; }
    }
}
