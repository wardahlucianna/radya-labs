using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.POI
{
    public class GetPOIRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string ProgrammeInqId { get; set; }
        //public string ClassId { get; set; }
        public string IdClassroom { get; set; }
    }
}
