using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselingServiceEntry
{
    public class GetCounselingServiceEntryStudentHistoryRequest : CollectionRequest
    {
        public string IdStudent { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdCounselingCategory { get; set; }
    }
}
