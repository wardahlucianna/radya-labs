using System;
using System.Collections.Generic;

namespace BinusSchool.Data.Model.School.FnPeriod.Period
{
    public class UpdatePeriodRequest
    {
        public string Id { get; set; }
        public IEnumerable<TermWithId> Terms { get; set; }
    }

    public class TermWithId : Term
    {
        public string Id { get; set; }
    }
}
