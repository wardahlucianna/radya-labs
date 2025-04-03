using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.Division
{
    public class GetDivisionCombinationRequest : CollectionRequest
    {
        public string IdTimetablePreferenceHeader { get; set; }
        public string IdParent { get; set; }
        public IEnumerable<string> IdChild { get; set; }
    }
}
