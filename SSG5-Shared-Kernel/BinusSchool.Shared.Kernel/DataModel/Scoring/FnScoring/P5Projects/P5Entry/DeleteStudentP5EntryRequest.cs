using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.P5Projects.P5Entry
{
    public class DeleteStudentP5EntryRequest
    {
        public string IdP5ProjectOutcomes {  get; set; }
        public string IdHomeroom { get; set; }
        public string IdStudent {  get; set; }
    }
}
