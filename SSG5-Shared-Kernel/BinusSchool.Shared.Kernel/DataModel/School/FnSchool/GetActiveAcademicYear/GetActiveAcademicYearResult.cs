using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.GetActiveAcademicYear
{
    public class GetActiveAcademicYearResult
    {
        public string AcademicYear { get; set; }
        public string AcademicYearId { get; set; }
        public int? Semester { get; set; }
        public string PreviousAcademicYear { get; set; }
        public string PreviousAcademicYearId { get; set; }
        public int? PreviousSemester { get; set; }
    }
}
