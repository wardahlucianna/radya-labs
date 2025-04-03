using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselingServiceEntry
{
    public class GetCounselingServiceEntryRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeRoom { get; set; }
        public string IdCounselingCategory { get; set; }
        public int? Semester { get; set; }
    }
}
