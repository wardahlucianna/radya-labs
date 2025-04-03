using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class GetCounselingServicesEntryByStudentResult
    {
        public string Id { get; set; }
        public string AcademicYear { get; set; }
        public string CounselingCategory { get; set; }
        public string CounselorName { get; set; }
        public DateTime CounselingDate { get; set; }
    }
}
