using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class GC3NotificationResult
    {
        public string AcademicYear { get; set; }
        public string StudentName { get; set; }
        public string IdStudent { get; set; }
        public string ReportedBy { get; set; }
        public string Date { get; set; }
        public string Note { get; set; }
        public string UserConsellor { get; set; }
        public string IdConsellor { get; set; }
        public string EmailConsellor { get; set; }
        public string DateUpdate { get; set; }
    }
}
