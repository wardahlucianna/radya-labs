using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class GetReportStudentToGcByStudentRequest:CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdUserStudent { get; set; }
    }
}
