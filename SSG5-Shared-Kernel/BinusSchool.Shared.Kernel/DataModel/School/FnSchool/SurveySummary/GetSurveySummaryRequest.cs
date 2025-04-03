using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.SurveySummary
{
    public class GetSurveySummaryRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string Semester { get; set; }
    }
}
