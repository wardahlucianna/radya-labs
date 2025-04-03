using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.P5Projects.P5Entry
{
    public class GetStudentP5EntryRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string IdTheme { get; set; }
        public string IdP5Project { get; set; }
    }
}
