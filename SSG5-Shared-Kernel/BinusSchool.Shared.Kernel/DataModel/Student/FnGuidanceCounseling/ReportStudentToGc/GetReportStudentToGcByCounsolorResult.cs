using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class GetReportStudentToGcByCounsolorResult 
    {
        public string Id { get; set; }
        public string AcademicYear { get; set; }
        public string StudentName { get; set; }
        public string BinusanId { get; set; }
        public string ReportedBy { get; set; }
        public DateTime ReportStudentDate { get; set; }
        public string ReportStudentNote { get; set; }
        public bool IsRead { get; set; }
        public bool IsCounselor { get; set; }
    }
}
