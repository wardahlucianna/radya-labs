using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class GetCounselingServicesEntryByStudentRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdUserStudent { get; set; }
        public string IdConselingCategory { get; set; }
    }
}
