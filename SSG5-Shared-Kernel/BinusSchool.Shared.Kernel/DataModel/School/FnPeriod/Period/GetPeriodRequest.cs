using System;
using System.Collections.Generic;

namespace BinusSchool.Data.Model.School.FnPeriod.Period
{
    public class GetPeriodRequest : CollectionSchoolRequest
    {
        public IEnumerable<string> IdAcadyear { get; set; }
        public IEnumerable<string> IdLevel { get; set; }
        public IEnumerable<string> IdGrade { get; set; }
    }
}
