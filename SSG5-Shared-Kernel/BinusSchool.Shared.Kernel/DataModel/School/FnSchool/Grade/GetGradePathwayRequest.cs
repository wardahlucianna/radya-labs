using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.GradePathways
{
    public class GetGradePathwayRequest : CollectionSchoolRequest
    {
        public IEnumerable<string> IdSchoolAcadyear { get; set; }
    }
}
