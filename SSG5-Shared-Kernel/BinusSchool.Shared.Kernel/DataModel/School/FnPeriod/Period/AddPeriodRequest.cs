using System;
using System.Collections.Generic;

namespace BinusSchool.Data.Model.School.FnPeriod.Period
{
    public class AddPeriodRequest
    {
        public string IdAcadyear { get; set; }
        public IEnumerable<string> IdGrades { get; set; }
        public IEnumerable<Term> Terms { get; set; }
    }

    public class Term
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime AttendanceStartDate { get; set; }
        public DateTime AttendanceEndDate { get; set; }
        public int? Semester { get; set; }
    }
}
