using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class GetReportStudentToGcByStudentResult
    {
        public string Id { get; set; }
        public string ReportedBy { get; set; }
        public DateTime ReportStudentDate { get; set; }
        public string ReportStudentNote { get; set; }
    }
}
