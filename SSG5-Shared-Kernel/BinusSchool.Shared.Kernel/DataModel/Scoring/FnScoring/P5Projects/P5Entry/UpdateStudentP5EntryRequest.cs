using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.P5Projects.P5Entry
{
    public class UpdateStudentP5EntryRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdHomeroom { get; set; }
        public string IdP5Project { get;set; }
        public string IdProjectOutcome { get; set; }
        public List<UpdateStudentP5Entry_StudentVm> Students { get; set; }
    }

    public class UpdateStudentP5Entry_StudentVm
    {
        public string IdStudent { get; set; }
        public int Predicate { get; set; }    
    }
}
