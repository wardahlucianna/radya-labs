using System;
using System.Collections.Generic;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class AddReportStudentToGcRequest
    {
        public string IdGcReportStudentLevel { get; set; }
        public string IdAcademicYear { get; set; }
        public DateTime Date { get; set; }
        public string IdLevel { get; set; }
        public string IdUserReport { get; set; }
        public List<string> IdGrades { get; set; }
        public List<AddReportStudentToGcData> Students { get; set; }
    }

    public class AddReportStudentToGcData
    {
        public string IdGcReportStudent {  get; set; }
        public string IdGrade { get; set; }
        public string IdStudent { get; set; }
        public string Note { get; set; }
    }
}
