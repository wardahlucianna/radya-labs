using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnPeriod.Period
{
    public class GetPeriodDetailResult : DetailResult2
    {
        public CodeWithIdVm Acadyear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public IEnumerable<TermDetail> Terms { get; set; }
    }

    public class TermDetail : CodeWithIdVm
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime AttendanceStartDate { get; set; }
        public DateTime AttendanceEndDate { get; set; }
        public int? Semester { get; set; }
    }
}
