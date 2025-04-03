using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.POI
{
    public class GetPOIParameterDescriptionResult
    {
        public string School { set; get; }
        public string AcademicYear { set; get; }
        public int Semester { set; get; }
        public string Level { set; get; }
        public string Grade { set; get; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
    }
}
