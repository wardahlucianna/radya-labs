using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.POI
{
    public class AddPOIRequest
    {
        public string IdStudent { get; set; }
        public string IdProgrammeInq { get; set; }
        public string Comment { get; set; }
        public string UserIn { get; set; }
    }
}
