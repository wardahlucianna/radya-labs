using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class UpdateReportStudentToGcRequest
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
    }
}
