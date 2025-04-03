using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class GC2NotificationRequest
    {
        public string Id { get; set; }
        public string oldNote { get; set; }
        public DateTime oldDate { get; set; }
    }
}
